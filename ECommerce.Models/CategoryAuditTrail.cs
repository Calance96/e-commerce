using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace ECommerce.Models
{
    public class CategoryAuditTrail
    {
        [Key]
        public long Id { get; set; }

        [Required]
        public long CategoryId { get; set; }

        [Required]
        [MaxLength(20)]
        public string Name { get; set; }

        [Required]
        public long ActionTypeId { get; set; }

        [ForeignKey("ActionTypeId")]
        public EntityActionType ActionType { get; set; }

        public string PerformedBy { get; set; }

        public DateTime PerformedDate { get; set; }
    }
}
