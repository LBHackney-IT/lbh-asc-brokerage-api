using System;
using System.Linq;
using System.Threading.Tasks;
using BrokerageApi.V1.Boundary.Request;
using BrokerageApi.V1.Gateways.Interfaces;
using BrokerageApi.V1.Infrastructure;
using BrokerageApi.V1.UseCase.Interfaces;

namespace BrokerageApi.V1.UseCase
{
    public class GetServiceUserByRequestUseCase : IGetServiceUserByRequestUseCase
    {
        private readonly IServiceUserGateway _serviceUserGateway;

        public GetServiceUserByRequestUseCase(IServiceUserGateway serviceUserGateway)
        {
            _serviceUserGateway = serviceUserGateway;
        }

        public async Task<ServiceUser> ExecuteAsync(GetServiceUserRequest request)
        {

            var serviceUser = await _serviceUserGateway.GetByRequestAsync(request);

            if (serviceUser is null)
            {
                throw new ArgumentException($"No service user found with the specified parameters");
            }

            return serviceUser;
        }
    }
}
