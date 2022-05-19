using System.Collections.Generic;
using System.Threading.Tasks;
using BrokerageApi.V1.Gateways.Interfaces;
using BrokerageApi.V1.Infrastructure;
using BrokerageApi.V1.UseCase.Interfaces;

namespace BrokerageApi.V1.UseCase
{
    public class GetCarePackagesByServiceUserIdUseCase : IGetCarePackagesByServiceUserIdUseCase
    {
        private readonly IServiceUserGateway _serviceUserGateway;

        public GetCarePackagesByServiceUserIdUseCase(IServiceUserGateway serviceUserGateway)
        {
            _serviceUserGateway = serviceUserGateway;
        }

        public async Task<IEnumerable<CarePackage>> ExecuteAsync(string serviceUserId)
        {
            return await _serviceUserGateway.GetByServiceUserIdAsync(serviceUserId);


        }
    }
}
