using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace ECommerce.Models
{
    public class ProductCategory
    {
        [Key]
        public long Id { get; set; }

        [Required]
        public long ProductId { get; set; }

        [Required]
        public long CategoryId { get; set; }

        [ForeignKey("ProductId")]
        public Product Product { get; set; }

        [ForeignKey("CategoryId")]
        public Category Category { get; set; }
    }
}
