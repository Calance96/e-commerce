using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace ECommerce.Models
{
    public class ProductAuditTrail
    {
        [Key]
        public long Id { get; set; }

        [Required]
        public long ProductId { get; set; }

        [Required]
        public string Name { get; set; }

        [Required]
        public string Description { get; set; }

        [Required]
        [Column(TypeName = "decimal(18,2)")]
        [Range(0.01, 99999.99)]
        public decimal Price { get; set; }

        public string ImageUrl { get; set; }

        public bool IsAvailable { get; set; }

        public string Categories { get; set; }

        [Required]
        public long ActionTypeId { get; set; }

        [ForeignKey("ActionTypeId")]
        public EntityActionType ActionType { get; set; }

        [Required]
        public string PerformedBy { get; set; }

        [Required]
        public DateTime PerformedDate { get; set; }
    }
}
