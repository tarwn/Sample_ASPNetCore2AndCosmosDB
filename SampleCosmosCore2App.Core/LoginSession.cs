using Newtonsoft.Json;
using System;

namespace SampleCosmosCore2App.Core
{
    public class LoginSession
    {
        [JsonProperty("id")]
        public string Id { get; set; }

        public string UserId { get; set; }

        public DateTime CreationTime { get; set; }
        public DateTime? LogoutTime { get; set; }
    }
}