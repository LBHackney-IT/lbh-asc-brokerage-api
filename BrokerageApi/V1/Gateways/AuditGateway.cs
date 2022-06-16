using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using BrokerageApi.V1.Gateways.Interfaces;
using BrokerageApi.V1.Infrastructure;
using BrokerageApi.V1.Infrastructure.AuditEvents;
using Newtonsoft.Json;
using X.PagedList;

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
        public IPagedList<AuditEvent> GetServiceUserAuditEvents(string socialCareId, int pageNumber, int pageCount)
        {
            var auditEvents = _context.AuditEvents
                .Include(ae => ae.Referral)
                .Where(ae => ae.SocialCareId == socialCareId)
                .OrderBy(e => e.CreatedAt)
                .ToPagedList(pageNumber, pageCount);

            return auditEvents;
        }

        private static string GetMessage(AuditEventType auditEventType)
        {
            return auditEventType switch
            {
                AuditEventType.ReferralBrokerAssignment => "Assigned to broker",
                AuditEventType.ReferralBrokerReassignment => "Reassigned to broker",
                AuditEventType.ElementEnded => "Element Ended",
                AuditEventType.ElementCancelled => "Element Cancelled",
                AuditEventType.ElementSuspended => "Element Suspended",
                AuditEventType.CarePackageEnded => "Care Package Ended",
                AuditEventType.CarePackageCancelled => "Care Package Cancelled",
                AuditEventType.CarePackageSuspended => "Care Package Suspended",
                AuditEventType.ReferralArchived => "Referral Archived",
                AuditEventType.CarePackageBudgetApproverAssigned => "Care Package Assigned To Budget Approver",
                AuditEventType.CarePackageApproved => "Care Package Approved",
                AuditEventType.ImportNote => "Import Note",
                AuditEventType.AmendmentRequested => "Amendment Requested",
                _ => throw new ArgumentOutOfRangeException(nameof(auditEventType), auditEventType, null)
            };
        }
    }
}
