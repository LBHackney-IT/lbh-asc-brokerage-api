using System.Collections.Generic;
using System.Threading.Tasks;
using BrokerageApi.V1.Gateways.Interfaces;
using BrokerageApi.V1.Infrastructure;
using BrokerageApi.V1.UseCase.Interfaces;

namespace BrokerageApi.V1.UseCase
{
    public class GetAllServicesUseCase : IGetAllServicesUseCase
    {
        private readonly IServiceGateway _serviceGateway;

        public GetAllServicesUseCase(IServiceGateway serviceGateway)
        {
            _serviceGateway = serviceGateway;
        }

        public async Task<IEnumerable<Service>> ExecuteAsync()
        {
            return await _serviceGateway.GetAllAsync();
        }
    }
}
