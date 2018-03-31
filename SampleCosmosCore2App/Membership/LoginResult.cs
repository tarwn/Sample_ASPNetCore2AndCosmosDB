using System;

namespace SampleCosmosCore2App.Membership
{
    public class LoginResult
    {
        public bool Failed { get; set; }

        public static LoginResult GetFailed()
        {
            return new LoginResult()
            {
                Failed = true
            };
        }

        public static LoginResult GetSuccess()
        {
            return new LoginResult()
            {
                Failed = false
            };
        }
    }
}
