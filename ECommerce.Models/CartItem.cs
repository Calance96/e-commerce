using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace ECommerce.Models
{
    public class CartItem
    {
        [Key]
        public long Id { get; set; }

        public string UserId { get; set; }
        
        [ForeignKey("UserId")]
        public ApplicationUser ApplicationUser { get; set; }
        
        public long ProductId { get; set; }

        [ForeignKey("ProductId")]
        public Product Product { get; set; }

        [Range(minimum: 1, maximum:999)]
        public int Quantity { get; set; }

        [NotMapped]
        public double Price { get; set; }

        public CartItem()
        {
            Quantity = 1;
        }
    }
}
