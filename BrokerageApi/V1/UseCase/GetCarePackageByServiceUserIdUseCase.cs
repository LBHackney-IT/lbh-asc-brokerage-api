using System;
using System.Threading.Tasks;
using BrokerageApi.V1.Gateways.Interfaces;
using BrokerageApi.V1.Infrastructure;
using BrokerageApi.V1.UseCase.Interfaces;

namespace BrokerageApi.V1.UseCase
{
    public class getCarePackagesByServiceUserIdUseCase : IGetCarePackagesByServiceUserIdUseCase
    {
        private readonly ICarePackageGateway _carePackageGateway;

        public getCarePackagesByServiceUserIdUseCase(ICarePackageGateway carePackageGateway)
        {
            _carePackageGateway = carePackageGateway;
        }

        public async Task<CarePackage> ExecuteAsync(int serviceUserId)
        {
            var carePackage = await _carePackageGateway.GetByServiceUserIdAsync(serviceUserId);

            if (carePackage is null)
            {
                throw new ArgumentNullException(nameof(serviceUserId), $"Care package not found for: {serviceUserId}");
            }

            return carePackage;
        }
    }
}
