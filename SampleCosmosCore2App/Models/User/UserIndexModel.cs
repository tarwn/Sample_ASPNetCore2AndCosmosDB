using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using SampleCosmosCore2App.Core.Users;

namespace SampleCosmosCore2App.Models.User
{
    public class UserIndexModel
    {
        public UserModel User { get; set; }
        public Dictionary<string, List<UserAuthenticationModel>> UserAuthentications { get; set; }
    }
}
