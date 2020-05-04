using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace ECommerce.Models
{
    public class OrderAuditTrail
    {
        [Key]
        public long Id { get; set; }

        [Required]
        public long OrderId { get; set; }

        [Required]
        public long OrderActionId { get; set; }

        [ForeignKey("OrderActionId")]
        public OrderAction OrderActionType { get; set; }

        public string PerformedBy { get; set; }

        public DateTime PerformedDate { get; set; }
    }
}
