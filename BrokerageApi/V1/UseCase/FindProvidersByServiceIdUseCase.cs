using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using BrokerageApi.V1.Gateways.Interfaces;
using BrokerageApi.V1.Infrastructure;
using BrokerageApi.V1.UseCase.Interfaces;

namespace BrokerageApi.V1.UseCase
{
    public class FindProvidersByServiceIdUseCase : IFindProvidersByServiceIdUseCase
    {
        private readonly IServiceGateway _serviceGateway;
        private readonly IProviderGateway _providerGateway;

        public FindProvidersByServiceIdUseCase(
            IServiceGateway serviceGateway,
            IProviderGateway providerGateway
        )
        {
            _serviceGateway = serviceGateway;
            _providerGateway = providerGateway;
        }

        public async Task<IEnumerable<Provider>> ExecuteAsync(int serviceId, string query)
        {
            var service = await _serviceGateway.GetByIdAsync(serviceId);

            if (service is null)
            {
                throw new ArgumentNullException(nameof(serviceId), $"Service not found for: {serviceId}");
            }

            return await _providerGateway.FindByServiceIdAsync(service.Id, query);
        }
    }
}
