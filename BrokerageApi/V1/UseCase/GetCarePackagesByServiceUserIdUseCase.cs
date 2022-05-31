using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BrokerageApi.V1.Gateways.Interfaces;
using BrokerageApi.V1.Infrastructure;
using BrokerageApi.V1.UseCase.Interfaces;


namespace BrokerageApi.V1.UseCase
{
    public class GetCarePackagesByServiceUserIdUseCase : IGetCarePackagesByServiceUserIdUseCase
    {
        private readonly ICarePackageGateway _carePackageGateway;

        public GetCarePackagesByServiceUserIdUseCase(ICarePackageGateway carePackageGateway)
        {
            _carePackageGateway = carePackageGateway;
        }

        public async Task<IEnumerable<CarePackage>> ExecuteAsync(string serviceUserId)
        {
            var carePackages = await _carePackageGateway.GetByServiceUserIdAsync(serviceUserId);

            if (!carePackages.Any())
            {
                throw new ArgumentException($"No care packages found for: {serviceUserId}");
            }
            return carePackages;
        }
    }
}
