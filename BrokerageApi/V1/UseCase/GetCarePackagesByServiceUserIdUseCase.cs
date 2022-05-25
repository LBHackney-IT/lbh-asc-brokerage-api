using System.Collections.Generic;
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
            return await _carePackageGateway.GetByServiceUserIdAsync(serviceUserId);


        }
    }
}
