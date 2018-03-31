using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace SampleCosmosCore2App.Core.Users
{
    public class LoginUser
    {
        [JsonProperty("id")]
        public string Id { get; set; }
        public string Username { get; set; }
        public string Email { get; set; }
        
        public string PasswordHash { get; set; }
    }
}
