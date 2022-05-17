using System;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using BrokerageApi.V1.Gateways.Interfaces;
using BrokerageApi.V1.Infrastructure;
using BrokerageApi.V1.Infrastructure.AuditEvents;
using Newtonsoft.Json;

namespace BrokerageApi.V1.Gateways
{
    public class AuditGateway : IAuditGateway
    {
        private readonly BrokerageContext _context;
        public AuditGateway(BrokerageContext context)
        {
            _context = context;
        }
        public async Task AddAuditEvent(AuditEventType type, string socialCareId, int userId, AuditMetadataBase metadata)
        {
            var auditEvent = new AuditEvent
            {
                EventType = type,
                SocialCareId = socialCareId,
                Message = GetMessage(type),
                UserId = userId,
                CreatedAt = _context.Clock.Now,
                Metadata = JsonConvert.SerializeObject(metadata)
            };

            await _context.AuditEvents.AddAsync(auditEvent);
            await _context.SaveChangesAsync();
        }

        private static string GetMessage(AuditEventType auditEventType)
        {
            return auditEventType switch
            {
                AuditEventType.ReferralBrokerAssignment => "Assigned to broker",
                AuditEventType.ReferralBrokerReassignment => "Reassigned to broker",
                _ => throw new ArgumentOutOfRangeException(nameof(auditEventType), auditEventType, null)
            };
        }
    }
}
