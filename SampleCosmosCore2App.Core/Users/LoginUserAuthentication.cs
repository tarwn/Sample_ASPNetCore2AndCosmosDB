using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace SampleCosmosCore2App.Core.Users
{
    public class LoginUserAuthentication
    {
        [JsonProperty("id")]
        public string Id { get; set; }

        public string UserId { get; set; }
        public AuthenticationScheme Scheme { get; set; }
        public string Identity { get; set; }
        public string Name { get; set; }
        public DateTime CreationTime { get; set; }
    }
}
