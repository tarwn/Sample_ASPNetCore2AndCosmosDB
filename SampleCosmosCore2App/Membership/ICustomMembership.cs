﻿using SampleCosmosCore2App.Membership.Data;
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

        Task<RegisterResult> RegisterAsync(string username, string email, string password);
        Task<RegisterResult> RegisterExternalAsync(string username, string email, string scheme, string identity, string identityName);
        Task<bool> IsUsernameAvailable(string username);
        Task<bool> IsAlreadyRegisteredAsync(string scheme, string identity);

        Task<LoginResult> LoginAsync(string username, string password);
        Task<LoginResult> LoginExternalAsync(string scheme, string identity);
        Task<ClaimsPrincipal> GetOneTimeLoginAsync(string scheme, string userAuthId, string identity);

        Task<bool> ValidateLoginAsync(ClaimsPrincipal principal);

        Task LogoutAsync();

        string GetSessionId(ClaimsPrincipal principal);
        Task<SessionDetails> GetSessionDetailsAsync(ClaimsPrincipal principal);
        Task<Dictionary<string, object>> DescribeUserForErrorAsync(ClaimsPrincipal principal);

        string GenerateAPIKey(string userId);
        Task<AuthenticationDetails> AddAuthenticationAsync(string userId, string scheme, string identity, string identityName);
        Task<RevocationDetails> RevokeAuthenticationAsync(string userId, string identity);
        
    }
}
