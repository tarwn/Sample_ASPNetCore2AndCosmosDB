using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SampleCosmosCore2App.Membership.Data
{
    public class RevocationDetails
    {
        public bool Failed { get; set; }
        public string Error { get; set; }

        public static RevocationDetails GetSuccess()
        {
            return new RevocationDetails()
            {
                Failed = false
            };
        }

        public static RevocationDetails GetFailed(string error)
        {
            return new RevocationDetails()
            {
                Failed = true,
                Error = error
            };
        }
    }
}
