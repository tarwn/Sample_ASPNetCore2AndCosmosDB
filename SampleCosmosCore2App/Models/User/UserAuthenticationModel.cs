using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace SampleCosmosCore2App.Models.User
{
    public class UserAuthenticationModel
    {
        public const string EMPTY_MASKED_VALUE = "(empty)";

        public string Id { get; set; }
        public string Identity { get; set; }
        public string Name { get; set; }
        public DateTime CreationTime { get; set; }

        public string GetMaskedIdentity()
        {
            if (string.IsNullOrEmpty(Identity))
            {
                return EMPTY_MASKED_VALUE;
            }
            else if (Identity.Length == 1)
            {
                return "X";
            }
            else if (Identity.Length < 4)
            {
                return Regex.Replace(Identity, "(?<=.).", "X");
            }
            else
            {
                return Regex.Replace(Identity, "(?<=..).", "X");
            }
        }
    }
}
