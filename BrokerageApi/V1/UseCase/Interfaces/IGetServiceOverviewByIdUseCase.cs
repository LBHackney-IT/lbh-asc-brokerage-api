using System.Threading.Tasks;
using BrokerageApi.V1.Infrastructure;

namespace BrokerageApi.V1.UseCase.Interfaces
{
    public interface IGetServiceOverviewByIdUseCase
    {
        public Task<ServiceOverview> ExecuteAsync(string socialCareId, int serviceId);
    }
}
