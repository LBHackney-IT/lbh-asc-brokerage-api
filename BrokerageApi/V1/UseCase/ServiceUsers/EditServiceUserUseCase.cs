using System;
using System.Linq;
using System.Threading.Tasks;
using BrokerageApi.V1.Boundary.Request;
using BrokerageApi.V1.Factories;
using BrokerageApi.V1.Gateways.Interfaces;
using BrokerageApi.V1.Infrastructure;
using BrokerageApi.V1.Controllers.Parameters;
using BrokerageApi.V1.UseCase.Interfaces.ServiceUsers;




namespace BrokerageApi.V1.UseCase.ServiceUsers
{
    public class EditServiceUserUseCase : IEditServiceUserUseCase
    {
        private readonly IServiceUserGateway _serviceUserGateway;
        private readonly IDbSaver _dbSaver;

        public EditServiceUserUseCase(
            IServiceUserGateway serviceUserGateway,
            IDbSaver dbSaver)
        {
            _serviceUserGateway = serviceUserGateway;
            _dbSaver = dbSaver;
        }


        public async Task<ServiceUser> ExecuteAsync(EditServiceUserRequest request)
        {

            var serviceUserRequestId = request.SocialCareId;

            var serviceUser = await _serviceUserGateway.GetBySocialCareIdAsync(serviceUserRequestId);
            if (serviceUser is null)
            {
                throw new ArgumentNullException(nameof(request), $"ServiceUser with ID {serviceUserRequestId} not found");
            }
            else
            {
                request.ToDatabase(serviceUser);
                await _dbSaver.SaveChangesAsync();
                return serviceUser;
            }
        }
    }
}
