using Microsoft.AspNet.Identity.EntityFramework;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace ECommerce.Models
{
    class ApplicationUser : IdentityUser
    {
        [Required]
        public string Name { get; set; }

        public string Address { get; set; }

        public string Role { get; set; }
    }
}
