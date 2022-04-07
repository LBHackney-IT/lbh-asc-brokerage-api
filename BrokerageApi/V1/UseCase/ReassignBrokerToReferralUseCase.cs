using System;
using System.Threading.Tasks;
using BrokerageApi.V1.Boundary.Request;
using BrokerageApi.V1.Gateways.Interfaces;
using BrokerageApi.V1.Infrastructure;
using BrokerageApi.V1.UseCase.Interfaces;

namespace BrokerageApi.V1.UseCase
{
    public class ReassignBrokerToReferralUseCase : IReassignBrokerToReferralUseCase
    {
        private readonly IReferralGateway _referralGateway;
        private readonly IDbSaver _dbSaver;

        public ReassignBrokerToReferralUseCase(IReferralGateway referralGateway, IDbSaver dbSaver)
        {
            _referralGateway = referralGateway;
            _dbSaver = dbSaver;
        }

        public async Task<Referral> ExecuteAsync(int referralId, AssignBrokerRequest request)
        {
            var referral = await _referralGateway.GetByIdAsync(referralId);

            if (referral is null)
            {
                throw new ArgumentException($"Referral not found for: {referralId}");
            }

            if (!CanBeReassigned(referral))
            {
                throw new InvalidOperationException($"Referral is not in a valid state for reassignment");
            }

            referral.AssignedTo = request.Broker;
            await _dbSaver.SaveChangesAsync();

            return referral;
        }

        private static bool CanBeReassigned(Referral referral)
        {
            return referral.Status == ReferralStatus.Assigned || referral.Status == ReferralStatus.InProgress;
        }
    }
}
