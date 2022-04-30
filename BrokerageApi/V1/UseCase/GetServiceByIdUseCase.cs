using System;
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
            var service = await _serviceGateway.GetByIdAsync(id);

            if (service is null)
            {
                throw new ArgumentNullException(nameof(id), $"Service not found for: {id}");
            }

            return service;
        }
    }
}
