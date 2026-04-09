using System.ComponentModel.DataAnnotations;

namespace MidStateShuttleService.Models
{
    public partial class Notification
    {
        [Key]
        public int Id { get; set; }

        /// <summary>
        /// The subject of the notification
        /// </summary>
        public string? Subject { get; set; }

        /// <summary>
        /// Time the notification was sent
        /// </summary>
        public DateTime? TimeSent { get; set; }

        /// <summary>
        /// The content of the notification
        /// </summary>
        public string? Body { get; set; }

        /// <summary>
        /// Whether the notification is archived
        /// </summary>
        public bool IsArchived { get; set; } = false;

        /// <summary>
        /// If the notification was for a request/registration use this
        /// </summary>
        public int? RegistrationId { get; set; }

        /// <summary>
        /// If the notification was for a feedback use this
        /// </summary>
        public int? FeedbackId { get; set; }

        /// <summary>
        /// If the notification was for a message use this
        /// </summary>
        public int? MessageId { get; set; }
    }
}
