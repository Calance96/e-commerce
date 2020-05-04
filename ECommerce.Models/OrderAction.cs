using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace ECommerce.Models
{
    public class OrderAction
    {
        [Key]
        public long Id { get; set; }

        [Required]
        [StringLength(maximumLength: 20)]
        public string ActionName { get; set; }
    }
}
