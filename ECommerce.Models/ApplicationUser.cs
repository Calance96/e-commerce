using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace ECommerce.Models
{
    public class ApplicationUser : IdentityUser
    {
        [Required]
        public string Name { get; set; }

        public string Address { get; set; }

        public string CreatedBy { get; set; }

        public DateTime CreatedAt { get; set; }


        [NotMapped]
        public string Role { get; set; }
    }
}
