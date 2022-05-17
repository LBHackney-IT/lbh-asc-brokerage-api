using System.Collections.Generic;
using System.Threading.Tasks;
using BrokerageApi.V1.Infrastructure.AuditEvents;
using PagedList.Core;

namespace BrokerageApi.V1.UseCase.Interfaces
{
    public interface IGetServiceUserAuditEventsUseCase
    {
        public IPagedList<AuditEvent> Execute(string socialCareId, int pageNumber, int pageSize);
    }

}
