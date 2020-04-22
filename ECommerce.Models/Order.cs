using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace ECommerce.Models
{
    public class Order
    {
        [Key]
        public long Id { get; set; }

        public string UserId { get; set; }

        [ForeignKey("UserId")]
        public ApplicationUser User { get; set; }

        [Required]
        public DateTime OrderDate { get; set; }
        
        [Required]
        public DateTime ShipDate { get; set; }

        [Required]
        [Column(TypeName = "decimal(18,2)")]
        public decimal OrderTotal { get; set; }

        public string TrackingNumber { get; set; }

        public string Carrier { get; set; }

        public string OrderStatus { get; set; }

        [Required]
        public string Name { get; set; }
        [Required]
        public string ShippingAddress { get; set; }
        [Required]
        public string BillingAddress { get; set; }
        [Required]
        public string PhoneNumber { get; set; }
    }
}
