using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SampleCosmosCore2App.Models.Shared
{
    public class SessionDetailsModel
    {
        public string Id { get; internal set; }
        public DateTime CreationTime { get; internal set; }
        public string Username { get; internal set; }
    }
}
