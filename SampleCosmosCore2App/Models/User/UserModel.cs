using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SampleCosmosCore2App.Models.User
{
    public class UserModel
    {
        public string Id { get;  set; }
        public string Username { get;  set; }
        public DateTime CreationTime { get;  set; }
    }
}
