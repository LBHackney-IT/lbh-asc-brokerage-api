using System;
using System.Threading.Tasks;
using BrokerageApi.V1.Gateways.Interfaces;
using BrokerageApi.V1.Infrastructure;
using BrokerageApi.V1.UseCase.Interfaces;

namespace BrokerageApi.V1.UseCase
{
    public class GetServiceOverviewByIdUseCase : IGetServiceOverviewByIdUseCase
    {
        private readonly IServiceUserGateway _serviceUserGateway;
        private readonly IServiceOverviewGateway _serviceOverviewGateway;

        public GetServiceOverviewByIdUseCase(
            IServiceUserGateway serviceUserGateway,
            IServiceOverviewGateway serviceOverviewGateway)
        {
            _serviceUserGateway = serviceUserGateway;
            _serviceOverviewGateway = serviceOverviewGateway;
        }

        public async Task<ServiceOverview> ExecuteAsync(string socialCareId, int serviceId)
        {
            var serviceUser = await _serviceUserGateway.GetBySocialCareIdAsync(socialCareId);

            if (serviceUser == null)
            {
                throw new ArgumentNullException(nameof(socialCareId), $"Service user not found for: {socialCareId}");
            }

            var serviceOverview = await _serviceOverviewGateway.GetBySocialCareIdAndServiceIdAsync(socialCareId, serviceId);

            if (serviceOverview == null)
            {
                throw new ArgumentNullException(nameof(serviceId), $"Service not found for: {serviceId}");
            }

            return serviceOverview;
        }
    }
}
