using System;

namespace SampleCosmosCore2App.Membership
{
    public class SessionDetails
    {
        public string Id { get; set; }
        public DateTime CreationTime { get; set; }
        public DateTime? LogoutTime { get; set; }

        public UserDetails User { get; set; }
    }
}