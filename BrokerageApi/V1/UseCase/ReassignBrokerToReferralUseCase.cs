using System;
using System.Threading.Tasks;
using BrokerageApi.V1.Boundary.Request;
using BrokerageApi.V1.Gateways.Interfaces;
using BrokerageApi.V1.Infrastructure;
using BrokerageApi.V1.Infrastructure.AuditEvents;
using BrokerageApi.V1.Services.Interfaces;
using BrokerageApi.V1.UseCase.Interfaces;

namespace BrokerageApi.V1.UseCase
{
    public class ReassignBrokerToReferralUseCase : IReassignBrokerToReferralUseCase
    {
        private readonly IReferralGateway _referralGateway;
        private readonly IAuditGateway _auditGateway;
        private readonly IUserService _userService;
        private readonly IDbSaver _dbSaver;
        private readonly IUserGateway _userGateway;

        public ReassignBrokerToReferralUseCase(IReferralGateway referralGateway,
            IAuditGateway auditGateway,
            IUserService userService,
            IDbSaver dbSaver,
            IUserGateway userGateway)
        {
            _referralGateway = referralGateway;
            _auditGateway = auditGateway;
            _userService = userService;
            _dbSaver = dbSaver;
            _userGateway = userGateway;
        }

        public async Task<Referral> ExecuteAsync(int referralId, AssignBrokerRequest request)
        {
            var referral = await _referralGateway.GetByIdAsync(referralId);

            if (referral is null)
            {
                throw new ArgumentNullException(nameof(referralId), $"Referral not found for: {referralId}");
            }

            if (!CanBeReassigned(referral))
            {
                throw new InvalidOperationException($"Referral is not in a valid state for reassignment");
            }

            var brokerUser = await _userGateway.GetByEmailAsync(request.Broker);

            if (brokerUser is null)
            {
                throw new ArgumentNullException(nameof(request), $"Broker not found for: {request.Broker}");
            }

            referral.AssignedBrokerEmail = request.Broker;
            await _dbSaver.SaveChangesAsync();

            await _auditGateway.AddAuditEvent(AuditEventType.ReferralBrokerReassignment, referral.SocialCareId, _userService.UserId, new ReferralAssignmentAuditEventMetadata
            {
                ReferralId = referral.Id,
                AssignedBrokerName = brokerUser.Name
            });

            return referral;
        }

        private static bool CanBeReassigned(Referral referral)
        {
            return referral.Status == ReferralStatus.Assigned || referral.Status == ReferralStatus.InProgress;
        }
    }
}
