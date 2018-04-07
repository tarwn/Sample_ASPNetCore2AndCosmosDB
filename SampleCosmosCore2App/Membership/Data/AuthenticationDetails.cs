using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SampleCosmosCore2App.Membership.Data
{
    public class AuthenticationDetails
    {
        public string Id { get; set; }
        public string Scheme { get; set; }
        public string Identity { get; set; }
        public string Name { get; set; }
        public DateTime CreationTime { get; set; }
    }
}
