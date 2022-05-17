using System.Collections.Generic;
using System.Threading.Tasks;
using BrokerageApi.V1.Infrastructure;

namespace BrokerageApi.V1.UseCase.Interfaces
{
    public interface IGetServiceOverviewUseCase
    {
        public Task<IEnumerable<Element>> ExecuteAsync(string socialCareId);
    }

}
