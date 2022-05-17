using BrokerageApi.V1.Gateways.Interfaces;
using BrokerageApi.V1.Infrastructure.AuditEvents;
using BrokerageApi.V1.UseCase.Interfaces;
using X.PagedList;

namespace BrokerageApi.V1.UseCase
{
    public class GetServiceUserAuditEventsUseCase : IGetServiceUserAuditEventsUseCase
    {
        private readonly IAuditGateway _auditGateway;

        public GetServiceUserAuditEventsUseCase(IAuditGateway auditGateway)
        {
            _auditGateway = auditGateway;
        }

        public IPagedList<AuditEvent> Execute(string socialCareId, int pageNumber, int pageSize)
        {
            return _auditGateway.GetServiceUserAuditEvents(socialCareId, pageNumber, pageSize);
        }
    }
}
