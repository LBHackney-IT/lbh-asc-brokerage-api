using System.ComponentModel.DataAnnotations;
using NodaTime;

namespace BrokerageApi.V1.Infrastructure.AuditEvents
{

    public class AuditEvent
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public string SocialCareId { get; set; }

        [Required]
        public string Message { get; set; }

        [Required]
        public AuditEventType EventType { get; set; }

        public string Metadata { get; set; }

        [Required]
        public Instant CreatedAt { get; set; }

        [Required]
        public int UserId { get; set; }

        [Required]
        public User User { get; set; }
    }

}
