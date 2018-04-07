using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Authentication;
using SampleCosmosCore2App.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using SampleCosmosCore2App.Core.Users;
using System.Security.Cryptography;
using SampleCosmosCore2App.Membership.Data;

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

        public async Task<RegisterResult> RegisterAsync(string userName, string email, string password)
        {
            var passwordHash = BCrypt.Net.BCrypt.HashPassword(password);
            var user = new LoginUser()
            {
                Username = userName,
                Email = email,
                PasswordHash = passwordHash,
                CreationTime = DateTime.UtcNow
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

        public async Task<RegisterResult> RegisterExternalAsync(string username, string email, string scheme, string identity, string identityName)
        {
            var user = new LoginUser()
            {
                Username = username,
                Email = email,
                CreationTime = DateTime.UtcNow
            };
            var userAuth = new LoginUserAuthentication()
            {
                Scheme = StringToScheme(scheme),
                Identity = identity,
                Name = identityName,
                CreationTime = DateTime.UtcNow
            };

            try
            {
                user = await _persistence.Users.CreateUserAsync(user);
            }
            catch (Exception)
            {
                //TODO reduce breadth of exception statement
                return RegisterResult.GetFailed("Username is already in use");
            }

            try
            {
                userAuth.UserId = user.Id;
                userAuth = await _persistence.Users.CreateUserAuthenticationAsync(userAuth);
            }
            catch (Exception)
            {
                // cleanup
                await _persistence.Users.DeleteUserAsync(user);
                throw;
            }


            // add in option of an email activation step and use options to provide redirect url

            await SignInAsync(user);

            return RegisterResult.GetSuccess();
        }

        private Core.Users.AuthenticationScheme StringToScheme(string scheme)
        {
            switch (scheme)
            {
                case "Twitter":
                    return Core.Users.AuthenticationScheme.Twitter;
                case "APIKey":
                    return Core.Users.AuthenticationScheme.APIKey;
                default:
                    throw new ArgumentException("Unrecognized sign-in scheme", scheme);
            }
        }

        public async Task<bool> IsUsernameAvailable(string username)
        {
            return await _persistence.Users.IsUsernameRegisteredAsync(username);
        }

        public async Task<bool> IsAlreadyRegisteredAsync(string scheme, string identity)
        {
            return await _persistence.Users.IsIdentityRegisteredAsync(StringToScheme(scheme), identity);
        }

        public async Task<LoginResult> LoginAsync(string userName, string password)
        {
            var user = await _persistence.Users.GetUserByUsernameAsync(userName);
            if (user == null)
            {
                return LoginResult.GetFailed();
            }

            if (!BCrypt.Net.BCrypt.Verify(password, user.PasswordHash))
            {
                return LoginResult.GetFailed();
            }

            // add validation that user is allowed to login

            // add in option of for multifactor and use options to provide redirect url

            await SignInAsync(user);

            return LoginResult.GetSuccess();
        }

        public async Task<LoginResult> LoginExternalAsync(string scheme, string identity)
        {
            var authScheme = StringToScheme(scheme);
            var user = await _persistence.Users.GetUserByAuthenticationAsync(authScheme, identity);
            if (user == null)
            {
                return LoginResult.GetFailed();
            }

            // add validation that user is allowed to login

            // add in option of for multifactor and use options to provide redirect url

            await SignInAsync(user);

            return LoginResult.GetSuccess();
        }

        public async Task<ClaimsPrincipal> GetOneTimeLoginAsync(string scheme, string userAuthId, string identity, string authenticationScheme)
        {
            var authScheme = StringToScheme(scheme);
            var userAuth = await _persistence.Users.GetUserAuthenticationAsync(userAuthId);

            // are the passed auth details valid?
            if (userAuth == null)
            {
                return null;
            }

            if (userAuth.Scheme != authScheme || !userAuth.Identity.Equals(identity, StringComparison.CurrentCultureIgnoreCase))
            {
                return null;
            }

            // is the user allowed to log in?
            var user = await _persistence.Users.GetUserAsync(userAuth.UserId);

            // add validation that user is allowed to login

            // create a claims principal
            var claimsIdentity = new ClaimsIdentity(authenticationScheme);
            claimsIdentity.AddClaim(new Claim("userId", userAuth.UserId));
            claimsIdentity.AddClaim(new Claim("userAuthId", userAuth.Id));
            return new ClaimsPrincipal(claimsIdentity);
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
            identity.AddClaim(new Claim("userId", session.UserId));
            identity.AddClaim(new Claim("sessionId", session.Id));
            await _context.HttpContext.SignInAsync(new ClaimsPrincipal(identity));
        }

        public async Task<bool> ValidateLoginAsync(ClaimsPrincipal principal)
        {
            var sessionId = GetSessionId(principal);
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

        public async Task LogoutAsync()
        {
            await _context.HttpContext.SignOutAsync();

            var sessionId = GetSessionId(_context.HttpContext.User);
            if (sessionId != null)
            {
                var session = await _persistence.Users.GetSessionAsync(sessionId);
                session.LogoutTime = DateTime.UtcNow;
                await _persistence.Users.UpdateSessionAsync(session);
            }
        }

        public string GetSessionId(ClaimsPrincipal principal)
        {
            return principal.FindFirstValue("sessionId");
        }

        public async Task<SessionDetails> GetSessionDetailsAsync(ClaimsPrincipal principal)
        {
            var sessionId = GetSessionId(principal);
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
        
        public string GenerateAPIKey(string userId)
        {
            var key = new byte[32];
            using (var generator = RandomNumberGenerator.Create())
            {
                generator.GetBytes(key);
            }
            return Convert.ToBase64String(key);
        }

        public async Task<AuthenticationDetails> AddAuthenticationAsync(string userId, string scheme, string identity, string identityName)
        {
            var userAuth = new LoginUserAuthentication()
            {
                UserId = userId,
                Scheme = StringToScheme(scheme),
                Identity = identity,
                Name = identityName,
                CreationTime = DateTime.UtcNow
            };

            userAuth = await _persistence.Users.CreateUserAuthenticationAsync(userAuth);

            return new AuthenticationDetails()
            {
                Id = userAuth.Id,
                Scheme = userAuth.Scheme.ToString(),
                Identity = userAuth.Identity,
                Name = userAuth.Name,
                CreationTime = userAuth.CreationTime
            };
        }

        public async Task<RevocationDetails> RevokeAuthenticationAsync(string userId, string identity)
        {
            var userAuth = await _persistence.Users.GetUserAuthenticationAsync(identity);
            if (!userAuth.UserId.Equals(userId))
            {
                return RevocationDetails.GetFailed("Could not find specified API Key for your account");
            }

            if (userAuth.Scheme == Core.Users.AuthenticationScheme.RevokedAPIKey)
            {
                return RevocationDetails.GetFailed("APIKey has already been revoked");
            }

            if (userAuth.Scheme != Core.Users.AuthenticationScheme.APIKey)
            {
                return RevocationDetails.GetFailed("Could not find specified API Key for your account");
            }

            userAuth.Scheme = Core.Users.AuthenticationScheme.RevokedAPIKey;
            await _persistence.Users.UpdateUserAuthenticationAsync(userAuth);

            return RevocationDetails.GetSuccess();
        }
    }
}
