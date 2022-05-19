using BrokerageApi.V1.Infrastructure.AuditEvents;
using Newtonsoft.Json.Linq;
using NodaTime;

namespace BrokerageApi.V1.Boundary.Response
{
    public class AuditEventResponse
    {
        public int Id { get; set; }
        public string SocialCareId { get; set; }
        public string Message { get; set; }
        public AuditEventType EventType { get; set; }
        public Instant CreatedAt { get; set; }
        public int UserId { get; set; }
        public JObject Metadata { get; set; }
    }
}
