using System;
using System.Threading.Tasks;
using BrokerageApi.V1.Gateways.Interfaces;
using BrokerageApi.V1.Infrastructure;
using BrokerageApi.V1.Services.Interfaces;
using BrokerageApi.V1.UseCase.Interfaces;

namespace BrokerageApi.V1.UseCase
{
    public class GetCurrentUserUseCase : IGetCurrentUserUseCase
    {
        private readonly IUserService _userService;
        private readonly IUserGateway _userGateway;
        public GetCurrentUserUseCase(
            IUserService userService,
            IUserGateway userGateway
        )
        {
            _userService = userService;
            _userGateway = userGateway;
        }
        public async Task<User> ExecuteAsync()
        {
            var email = _userService.Email;

            var user = await _userGateway.GetByEmailAsync(email);

            if (user is null)
            {
                throw new ArgumentException($"User not found for: {email}");
            }

            return user;
        }
    }
}
