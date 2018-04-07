using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace SampleCosmosCore2App.Models.Account
{
    public class RegisterModel
    {
        [Required]
        [StringLength(72)]
        public string UserName { get; set; }

        [Required]
        [StringLength(72)]
        [MinLength(8)]
        public string Password { get; set; }

        [Required]
        [EmailAddress]
        public string Email { get; set; }
    }
}
