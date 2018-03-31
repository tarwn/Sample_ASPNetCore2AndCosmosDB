using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace SampleCosmosCore2App.Membership
{
    public interface ICustomMembership
    {
        CustomMembershipOptions Options { get; }

        Task<RegisterResult> RegisterAsync(string userName, string email, string password);
        Task<LoginResult> LoginAsync(string userName, string password);
        Task LogoutAsync();
        Task<bool> ValidateLoginAsync(ClaimsPrincipal principal);

        Task<SessionDetails> GetSessionDetailsAsync(ClaimsPrincipal principal);
    }
}
