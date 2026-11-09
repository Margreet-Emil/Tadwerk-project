using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace RecyclingApp.Models
{
    public class PickupRequest
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public string UserId { get; set; }

        public string? CollectorId { get; set; }

        public string Status { get; set; } = "Pending";

        public DateTime ScheduledAt { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal TotalValue { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.Now;

        // Customer must accept the accuracy & privacy policy before submitting
        public bool PrivacyAccepted { get; set; }

        public DateTime? PrivacyAcceptedAt { get; set; }

        // Fee recorded when the collector reports that the waste does not match the request
        [Column(TypeName = "decimal(18,2)")]
        public decimal PenaltyAmount { get; set; }

        public string? CollectorNote { get; set; }

        [ForeignKey(nameof(UserId))]
        public ApplicationUser User { get; set; }

        [ForeignKey(nameof(CollectorId))]
        public ApplicationUser? Collector { get; set; }

        public ICollection<RequestItem> RequestItems { get; set; }
    }
}
