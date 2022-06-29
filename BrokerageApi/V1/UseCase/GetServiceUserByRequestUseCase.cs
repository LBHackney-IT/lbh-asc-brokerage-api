using System;
using System.Threading.Tasks;
using BrokerageApi.V1.Controllers.Parameters;
using BrokerageApi.V1.Gateways.Interfaces;
using BrokerageApi.V1.Infrastructure;
using BrokerageApi.V1.UseCase.Interfaces;
using System.Collections.Generic;


namespace BrokerageApi.V1.UseCase
{
    public class GetServiceUserByRequestUseCase : IGetServiceUserByRequestUseCase
    {
        private readonly IServiceUserGateway _serviceUserGateway;

        public GetServiceUserByRequestUseCase(IServiceUserGateway serviceUserGateway)
        {
            _serviceUserGateway = serviceUserGateway;
        }

        public async Task<IEnumerable<ServiceUser>> ExecuteAsync(GetServiceUserRequest request)
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
