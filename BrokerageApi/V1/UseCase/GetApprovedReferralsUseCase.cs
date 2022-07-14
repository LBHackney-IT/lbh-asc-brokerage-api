using System.Collections.Generic;
using System.Threading.Tasks;
using BrokerageApi.V1.Gateways.Interfaces;
using BrokerageApi.V1.Infrastructure;
using BrokerageApi.V1.Services.Interfaces;
using BrokerageApi.V1.UseCase.Interfaces;

namespace BrokerageApi.V1.UseCase
{
    public class GetApprovedReferralsUseCase : IGetApprovedReferralsUseCase
    {
        private readonly IReferralGateway _referralGateway;

        public GetApprovedReferralsUseCase(IReferralGateway referralGateway)
        {
            _referralGateway = referralGateway;
        }

        public async Task<IEnumerable<Referral>> ExecuteAsync()
        {
            return await _referralGateway.GetApprovedAsync();
        }
    }
}
