using System.Collections.Generic;
using System.Threading.Tasks;
using BrokerageApi.V1.Gateways.Interfaces;
using BrokerageApi.V1.Infrastructure;
using BrokerageApi.V1.UseCase.Interfaces;

namespace BrokerageApi.V1.UseCase
{
    public class GetAllUsersUseCase : IGetAllUsersUseCase
    {
        private readonly IUserGateway _userGateway;

        public GetAllUsersUseCase(IUserGateway userGateway)
        {
            _userGateway = userGateway;
        }

        public async Task<IEnumerable<User>> ExecuteAsync(UserRole? role = null)
        {
            return await _userGateway.GetAllAsync(role);
        }
    }
}
