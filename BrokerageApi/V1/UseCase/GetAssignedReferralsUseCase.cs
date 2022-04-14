using System.Collections.Generic;
using System.Threading.Tasks;
using BrokerageApi.V1.Gateways.Interfaces;
using BrokerageApi.V1.Infrastructure;
using BrokerageApi.V1.UseCase.Interfaces;

namespace BrokerageApi.V1.UseCase
{
    public class GetAssignedReferralsUseCase : IGetAssignedReferralsUseCase
    {
        private readonly IReferralGateway _referralGateway;

        public GetAssignedReferralsUseCase(IReferralGateway referralGateway)
        {
            _referralGateway = referralGateway;
        }

        public async Task<IEnumerable<Referral>> ExecuteAsync(string email, ReferralStatus? status = null)
        {
            return await _referralGateway.GetAssignedAsync(email, status);
        }
    }
}
