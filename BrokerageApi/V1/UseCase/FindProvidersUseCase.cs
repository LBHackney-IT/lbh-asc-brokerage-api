using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using BrokerageApi.V1.Gateways.Interfaces;
using BrokerageApi.V1.Infrastructure;
using BrokerageApi.V1.UseCase.Interfaces;

namespace BrokerageApi.V1.UseCase
{
    public class FindProvidersUseCase : IFindProvidersUseCase
    {
        private readonly IProviderGateway _providerGateway;

        public FindProvidersUseCase(IProviderGateway providerGateway)
        {
            _providerGateway = providerGateway;
        }

        public async Task<IEnumerable<Provider>> ExecuteAsync(string query)
        {
            return await _providerGateway.FindAsync(query);
        }
    }
}
