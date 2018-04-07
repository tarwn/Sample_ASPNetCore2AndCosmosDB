using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SampleCosmosCore2App.Membership.Data
{
    public class RegisterResult
    {
        public bool Failed { get; set; }
        public string ErrorMessage { get; set; }

        public static RegisterResult GetFailed(string message)
        {
            return new RegisterResult()
            {
                Failed = true,
                ErrorMessage = message
            };
        }

        public static RegisterResult GetSuccess()
        {
            return new RegisterResult()
            {
                Failed = false
            };
        }
    }
}
