using System.Collections.Generic;
using System.Threading.Tasks;
using BrokerageApi.V1.Gateways.Interfaces;
using BrokerageApi.V1.Infrastructure;
using BrokerageApi.V1.Services.Interfaces;
using BrokerageApi.V1.UseCase.Interfaces;

namespace BrokerageApi.V1.UseCase
{
    public class GetAssignedReferralsUseCase : IGetAssignedReferralsUseCase
    {
        private readonly IReferralGateway _referralGateway;
        private readonly IUserService _userService;

        public GetAssignedReferralsUseCase(IReferralGateway referralGateway, IUserService userService)
        {
            _referralGateway = referralGateway;
            _userService = userService;
        }

        public async Task<IEnumerable<Referral>> ExecuteAsync(ReferralStatus? status = null)
        {
            return await _referralGateway.GetAssignedAsync(_userService.Name, status);
        }
    }
}
