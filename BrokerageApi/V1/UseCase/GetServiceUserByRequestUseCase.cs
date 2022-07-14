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
        private readonly IElementGateway _elementGateway;


        public GetServiceUserByRequestUseCase(IServiceUserGateway serviceUserGateway, IElementGateway elementGateway)
        {
            _serviceUserGateway = serviceUserGateway;
            _elementGateway = elementGateway;

        }

        public async Task<IEnumerable<ServiceUser>> ExecuteAsync(GetServiceUserRequest request)
        {
            var provider = request.ProviderId;
            if (provider != null)
            {
                //retrieving the elements for this provider
                var elements = await _elementGateway.GetByProviderIdAsync(provider);
                var serviceUsers = new List<ServiceUser>();
                foreach (var element in elements)
                {
                    //for each element found passing the socialcareid to the gateway 
                    var thisRequest = new GetServiceUserRequest();
                    thisRequest.SocialCareId = element.SocialCareId;
                    var serviceUser = await _serviceUserGateway.GetByRequestAsync(thisRequest);
                    foreach (var thisServiceUser in serviceUser)
                    {
                        //adding each service user found to the list
                        serviceUsers.Add(thisServiceUser);
                    }

                }
                return serviceUsers;

            }
            else
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
}
