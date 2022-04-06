using System;
using System.Threading.Tasks;
using BrokerageApi.V1.Boundary.Request;
using BrokerageApi.V1.Gateways.Interfaces;
using BrokerageApi.V1.Infrastructure;
using BrokerageApi.V1.UseCase.Interfaces;

namespace BrokerageApi.V1.UseCase
{
    public class AssignBrokerToReferralUseCase : IAssignBrokerToReferralUseCase
    {
        private readonly IReferralGateway _referralGateway;
        private readonly IDbSaver _dbSaver;

        public AssignBrokerToReferralUseCase(IReferralGateway referralGateway, IDbSaver dbSaver)
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

            if (referral.Status != ReferralStatus.Unassigned)
            {
                throw new InvalidOperationException($"Referral is not in a valid state for assignment");
            }

            referral.Status = ReferralStatus.Assigned;
            referral.AssignedTo = request.Broker;
            await _dbSaver.SaveChangesAsync();

            return referral;
        }
    }
}
