using System;
using System.Threading.Tasks;
using BrokerageApi.V1.Gateways.Interfaces;
using BrokerageApi.V1.UseCase.Interfaces.CarePackages;

namespace BrokerageApi.V1.UseCase.CarePackages
{
    public class GetCarePackageByIdUseCase : IGetCarePackageByIdUseCase
    {
        private readonly ICarePackageGateway _carePackageGateway;

        public GetCarePackageByIdUseCase(ICarePackageGateway carePackageGateway)
        {
            _carePackageGateway = carePackageGateway;
        }

        public async Task<Infrastructure.CarePackage> ExecuteAsync(int id)
        {
            var carePackage = await _carePackageGateway.GetByIdAsync(id);

            if (carePackage is null)
            {
                throw new ArgumentNullException(nameof(id), $"Care package not found for: {id}");
            }

            return carePackage;
        }
    }
}
