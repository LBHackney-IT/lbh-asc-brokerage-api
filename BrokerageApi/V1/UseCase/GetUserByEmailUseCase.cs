using System.Threading.Tasks;
using BrokerageApi.V1.Gateways.Interfaces;
using BrokerageApi.V1.Infrastructure;
using BrokerageApi.V1.UseCase.Interfaces;

namespace BrokerageApi.V1.UseCase
{
    public class GetUserByEmailUseCase : IGetUserByEmailUseCase
    {
        private readonly IUserGateway _userGateway;

        public GetUserByEmailUseCase(IUserGateway userGateway)
        {
            _userGateway = userGateway;
        }

        public async Task<User> ExecuteAsync(string email)
        {
            return await _userGateway.GetByEmailAsync(email);
        }
    }
}
