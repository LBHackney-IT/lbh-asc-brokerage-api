using System.Collections.Generic;
using System.Threading.Tasks;
using BrokerageApi.V1.Gateways.Interfaces;
using BrokerageApi.V1.Infrastructure;
using BrokerageApi.V1.UseCase.Interfaces;

namespace BrokerageApi.V1.UseCase
{
    public class GetCurrentReferralsUseCase : IGetCurrentReferralsUseCase
    {
        private readonly IReferralGateway _referralGateway;

        public GetCurrentReferralsUseCase(IReferralGateway referralGateway)
        {
            _referralGateway = referralGateway;
        }

        public async Task<IEnumerable<Referral>> ExecuteAsync(ReferralStatus? status = null)
        {
            return await _referralGateway.GetCurrentAsync(status);
        }
    }
}
