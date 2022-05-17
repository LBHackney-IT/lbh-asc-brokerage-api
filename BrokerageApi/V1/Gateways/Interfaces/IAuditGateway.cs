using System.Threading.Tasks;
using BrokerageApi.V1.Infrastructure.AuditEvents;
using X.PagedList;

namespace BrokerageApi.V1.Gateways.Interfaces
{
    public interface IAuditGateway
    {
        public Task AddAuditEvent(AuditEventType type, string socialCareId, int userId, AuditMetadataBase metadata);
        public IPagedList<AuditEvent> GetServiceUserAuditEvents(string socialCareId, int pageNumber, int pageCount);
    }

}
