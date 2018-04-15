using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SampleCosmosCore2App.Models.Error
{
    public class DescriptiveError
    {
        public DescriptiveError(string message)
        {
            Message = message;
        }

        public string Message { get; set; }
    }
}
