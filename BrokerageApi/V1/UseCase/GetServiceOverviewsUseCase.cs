using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BrokerageApi.V1.Gateways.Interfaces;
using BrokerageApi.V1.Infrastructure;
using BrokerageApi.V1.UseCase.Interfaces;

namespace BrokerageApi.V1.UseCase
{
    public class GetServiceOverviewsUseCase : IGetServiceOverviewsUseCase
    {
        private readonly IServiceUserGateway _serviceUserGateway;
        private readonly IServiceOverviewGateway _serviceOverviewGateway;

        public GetServiceOverviewsUseCase(
            IServiceUserGateway serviceUserGateway,
            IServiceOverviewGateway serviceOverviewGateway)
        {
            _serviceUserGateway = serviceUserGateway;
            _serviceOverviewGateway = serviceOverviewGateway;
        }

        public async Task<IEnumerable<ServiceOverview>> ExecuteAsync(string socialCareId)
        {
            var serviceUser = await _serviceUserGateway.GetBySocialCareIdAsync(socialCareId);

            if (serviceUser == null)
            {
                throw new ArgumentNullException(nameof(socialCareId), $"Service user not found for: {socialCareId}");
            }

            return await _serviceOverviewGateway.GetBySocialCareIdAsync(socialCareId);
        }
    }
}
