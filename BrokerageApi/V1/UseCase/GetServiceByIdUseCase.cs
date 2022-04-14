using System.Threading.Tasks;
using BrokerageApi.V1.Gateways.Interfaces;
using BrokerageApi.V1.Infrastructure;
using BrokerageApi.V1.UseCase.Interfaces;

namespace BrokerageApi.V1.UseCase
{
    public class GetServiceByIdUseCase : IGetServiceByIdUseCase
    {
        private readonly IServiceGateway _serviceGateway;

        public GetServiceByIdUseCase(IServiceGateway serviceGateway)
        {
            _serviceGateway = serviceGateway;
        }

        public async Task<Service> ExecuteAsync(int id)
        {
            return await _serviceGateway.GetByIdAsync(id);
        }
    }
}
