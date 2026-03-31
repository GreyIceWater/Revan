using System;
using System.ComponentModel.DataAnnotations;

namespace MidStateShuttleService.Models
{
    public class MailItem
    {
        [Key]
        public int MailItemId { get; set; }

        [Required]
        [StringLength(100)]
        public string SenderName { get; set; }

        [Required]
        [StringLength(100)]
        public string RecipientName { get; set; }

        [Required]
        [StringLength(100)]
        public string PickupLocation { get; set; }

        [Required]
        [StringLength(100)]
        public string DropoffLocation { get; set; }

        [Required]
        [StringLength(100)]
        public string MailType { get; set; }

        [StringLength(250)]
        public string? TrackingNumber { get; set; }

        [StringLength(500)]
        public string? Notes { get; set; }

        // System fields (NOT user input)
        public DateTime SubmittedAt { get; set; } = DateTime.UtcNow;

        [StringLength(100)]
        public string? SubmittedBy { get; set; }

        public bool IsActive { get; set; } = true;
    }
}