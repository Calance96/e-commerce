using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace ECommerce.Models
{
    public class OrderItem
    {
        [Key]
        public long Id { get; set; } 
        
        [Required]
        public long OrderId { get; set; }
        
        [ForeignKey("OrderId")]
        public Order Order { get; set; }

        [Required]
        public long ProductId { get; set; }

        [ForeignKey("ProductId")]
        public Product Product { get; set; }

        public int Quantity { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal Price { get; set; }
    }
}
