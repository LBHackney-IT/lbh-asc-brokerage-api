using System;
using System.Threading.Tasks;
using BrokerageApi.V1.Gateways.Interfaces;
using BrokerageApi.V1.Infrastructure;
using BrokerageApi.V1.UseCase.Interfaces;

namespace BrokerageApi.V1.UseCase
{
    public class GetReferralByIdUseCase : IGetReferralByIdUseCase
    {
        private readonly IReferralGateway _referralGateway;

        public GetReferralByIdUseCase(IReferralGateway referralGateway)
        {
            _referralGateway = referralGateway;
        }

        public async Task<Referral> ExecuteAsync(int id)
        {
            var referral = await _referralGateway.GetByIdAsync(id);

            if (referral is null)
            {
                throw new ArgumentNullException(nameof(id), $"Referral not found for: {id}");
            }

            return referral;
        }
    }
}
