using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace SampleCosmosCore2App.Membership
{
    public class CustomMembershipOptions
    {
        public string DefaultPathAfterLogin { get; set; }
        public string DefaultPathAfterLogout { get; set; }

        public string InteractiveAuthenticationType { get; set; }
        public string OneTimeAuthenticationType { get; set; }
    }
}
