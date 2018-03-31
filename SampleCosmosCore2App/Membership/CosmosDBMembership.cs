using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Authentication;
using SampleCosmosCore2App.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using SampleCosmosCore2App.Core.Users;

namespace SampleCosmosCore2App.Membership
{
    public class CosmosDBMembership : ICustomMembership
    {
        private IHttpContextAccessor _context;
        private Persistence _persistence;

        public CosmosDBMembership(IHttpContextAccessor context, CustomMembershipOptions options, Persistence persistence)
        {
            _context = context;
            _persistence = persistence;
            Options = options;
        }

        public CustomMembershipOptions Options { get; private set; }

        public async Task<SessionDetails> GetSessionDetailsAsync(ClaimsPrincipal principal)
        {
            var sessionId = principal.FindFirstValue("sessionId");
            if (sessionId == null)
            {
                return null;
            }

            var session = await _persistence.Users.GetSessionAsync(sessionId);
            var user = await _persistence.Users.GetUserAsync(session.UserId);

            return new SessionDetails()
            {
             Id = session.Id,
              CreationTime = session.CreationTime,
              LogoutTime = session.LogoutTime,
              User = new UserDetails()
              {
                  Id = user.Id,
                  Username = user.Username,
                  Email = user.Email
              }
            };
        }

        public async Task<LoginResult> LoginAsync(string userName, string password)
        {
            var user = await _persistence.Users.GetUserByUsernameAsync(userName);
            if (user == null)
            {
                return LoginResult.GetFailed();
            }

            //TODO: use bcrypt or similar to check password
            if (user.PasswordHash != password)
            {
                return LoginResult.GetFailed();
            }

            // add in option of for multifactor and use options to provide redirect url

            await SignInAsync(user);

            return LoginResult.GetSuccess();
        }
        
        public async Task LogoutAsync()
        {
            await _context.HttpContext.SignOutAsync();

            var sessionId = _context.HttpContext.User.FindFirstValue("sessionId");
            if (sessionId != null)
            {
                var session = await _persistence.Users.GetSessionAsync(sessionId);
                session.LogoutTime = DateTime.UtcNow;
                await _persistence.Users.UpdateSessionAsync(session);
            }
        }

        public async Task<RegisterResult> RegisterAsync(string userName, string email, string password)
        {
            //TODO: actually hash password
            var user = new LoginUser()
            {
                Username = userName,
                Email = email,
                PasswordHash = password
            };

            try
            {
                user = await _persistence.Users.CreateUserAsync(user);
            }
            catch (Exception exc)
            {
                //TODO reduce breadth of exception statement
                return RegisterResult.GetFailed("Username is already in use");
            }

            // add in option of an email activation step and use options to provide redirect url

            await SignInAsync(user);

            return RegisterResult.GetSuccess();
        }

        public async Task<bool> ValidateLoginAsync(ClaimsPrincipal principal)
        {
            var sessionId = principal.FindFirstValue("sessionId");
            if (sessionId == null)
            {
                return false;
            }

            var session = await _persistence.Users.GetSessionAsync(sessionId);
            if (session.LogoutTime.HasValue)
            {
                return false;
            }

            // add in options like updating it with a last seen time, expiration, etc
            // add in options like IP Address roaming check

            return true;
        }

        private async Task SignInAsync(LoginUser user)
        {
            // key the login to a server-side session id to make it easy to invalidate later
            var session = new LoginSession()
            {
                UserId = user.Id,
                CreationTime = DateTime.UtcNow
            };
            session = await _persistence.Users.CreateSessionAsync(session);

            var identity = new ClaimsIdentity(Options.AuthenticationType);
            identity.AddClaim(new Claim("sessionId", session.Id));
            await _context.HttpContext.SignInAsync(new ClaimsPrincipal(identity));
        }
    }
}
