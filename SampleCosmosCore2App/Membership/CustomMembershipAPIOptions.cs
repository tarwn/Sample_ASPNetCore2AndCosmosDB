using Microsoft.AspNetCore.Authentication;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SampleCosmosCore2App.Membership
{
    public class CustomMembershipAPIOptions : AuthenticationSchemeOptions
    {
        private static string DefaultScheme = "CustomMembershipAPIKey";

        public CustomMembershipAPIOptions()
        {
            Scheme = DefaultScheme;
        }

        public string Scheme { get; set; }
        public string Realm { get; set; }

    }
}
