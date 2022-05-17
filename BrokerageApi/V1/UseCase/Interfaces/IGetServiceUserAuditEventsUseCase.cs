using BrokerageApi.V1.Infrastructure.AuditEvents;
using X.PagedList;

namespace BrokerageApi.V1.UseCase.Interfaces
{
    public interface IGetServiceUserAuditEventsUseCase
    {
        public IPagedList<AuditEvent> Execute(string socialCareId, int pageNumber, int pageSize);
    }

}
