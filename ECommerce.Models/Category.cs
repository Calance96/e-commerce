using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
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

        public string CreatedBy { get; set; }

        public DateTime CreatedAt { get; set; }

        [NotMapped]
        public string UpdatedBy { get; set; }

        [NotMapped]
        public DateTime UpdatedAt { get; set; }
    }
}
