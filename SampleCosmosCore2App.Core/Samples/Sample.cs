using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace SampleCosmosCore2App.Core.Samples
{
    public class Sample
    {
        [JsonProperty(PropertyName = "id")]
        public string Id { get; set; }

        public string Content { get; set; }
    }
}
