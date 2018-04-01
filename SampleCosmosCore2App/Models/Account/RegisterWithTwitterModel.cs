using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace SampleCosmosCore2App.Models.Account
{
    public class RegisterWithTwitterModel
    {
        [Required]
        [StringLength(72)]
        public string UserName { get; set; }

        [Required]
        [EmailAddress]
        public string Email { get; set; }

        // Below are set from twitter SSO and should not be modified by user

        [Required]
        public string TwitterId { get; set; }

        [Required]
        public string TwitterUsername { get; set; }
    }
}
