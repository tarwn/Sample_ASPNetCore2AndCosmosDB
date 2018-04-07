using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.Net.Http.Headers;
using SampleCosmosCore2App.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text.Encodings.Web;
using System.Threading.Tasks;

namespace SampleCosmosCore2App.Membership
{
    public class CustomMembershipAPIAuthHandler : AuthenticationHandler<CustomMembershipAPIOptions>
    {
        private ICustomMembership _membership;

        public CustomMembershipAPIAuthHandler(ICustomMembership membership, IOptionsMonitor<CustomMembershipAPIOptions> options, ILoggerFactory logger, UrlEncoder encoder, ISystemClock clock)
            : base(options, logger, encoder, clock)
        {
            _membership = membership;
        }

        protected override async Task<AuthenticateResult> HandleAuthenticateAsync()
        {
            // Is this relevant to us?
            if (!Request.Headers.TryGetValue(HeaderNames.Authorization, out var authorization))
            {
                return AuthenticateResult.NoResult();
            }

            var actualAuthValue = authorization.FirstOrDefault(s => s.StartsWith(Options.Scheme, StringComparison.CurrentCultureIgnoreCase));
            if (actualAuthValue == null)
            {
                return AuthenticateResult.NoResult();
            }

            // Is it a good pair?
            var apiPair = actualAuthValue.Substring(Options.Scheme.Length + 1);
            var apiValues = apiPair.Split(':', 2);
            if (apiValues.Length != 2 || String.IsNullOrEmpty(apiValues[0]) || String.IsNullOrEmpty(apiValues[1]))
            {
                return AuthenticateResult.Fail($"Invalid authentication format, expected '{Options.Scheme} id:secret'");
            }

            var principal = await _membership.GetOneTimeLoginAsync("APIKey", apiValues[0], apiValues[1], Options.Scheme);
            if (principal == null)
            {
                return AuthenticateResult.Fail("Invalid authentication provided, access denied.");
            }

            var ticket = new AuthenticationTicket(principal, Options.Scheme);
            return AuthenticateResult.Success(ticket);
        }

        protected override async Task HandleChallengeAsync(AuthenticationProperties properties)
        {
            Response.Headers["WWW-Authenticate"] = $"Basic realm=\"{Options.Realm}\", charset=\"UTF-8\"";
            await base.HandleChallengeAsync(properties);
        }

        protected override Task HandleForbiddenAsync(AuthenticationProperties properties)
        {
            return base.HandleForbiddenAsync(properties);
        }
    }
}
