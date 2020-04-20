using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace ECommerce.Models
{
    public class Category
    {
        [Key]
        public long Id { get; set; }

        [Required]
        [Display(Name="Category Name")]
        [MaxLength(20)]
        public string Name { get; set; }
    }
}
