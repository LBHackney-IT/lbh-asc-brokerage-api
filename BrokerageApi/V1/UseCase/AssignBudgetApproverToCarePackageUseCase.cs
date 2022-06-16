using System;
using System.Linq;
using System.Threading.Tasks;
using BrokerageApi.V1.Gateways.Interfaces;
using BrokerageApi.V1.Infrastructure;
using BrokerageApi.V1.Infrastructure.AuditEvents;
using BrokerageApi.V1.Services.Interfaces;
using BrokerageApi.V1.UseCase.Interfaces;

namespace BrokerageApi.V1.UseCase
{
    public class AssignBudgetApproverToCarePackageUseCase : IAssignBudgetApproverToCarePackageUseCase
    {
        private readonly ICarePackageGateway _carePackageGateway;
        private readonly IReferralGateway _referralGateway;
        private readonly IUserGateway _userGateway;
        private readonly IUserService _userService;
        private readonly IAuditGateway _auditGateway;
        private readonly IDbSaver _dbSaver;
        public AssignBudgetApproverToCarePackageUseCase(ICarePackageGateway carePackageGateway,
            IReferralGateway referralGateway,
            IUserGateway userGateway,
            IUserService userService,
            IAuditGateway auditGateway,
            IDbSaver dbSaver)
        {
            _carePackageGateway = carePackageGateway;
            _referralGateway = referralGateway;
            _userGateway = userGateway;
            _userService = userService;
            _auditGateway = auditGateway;
            _dbSaver = dbSaver;
        }

        public async Task ExecuteAsync(int referralId, string budgetApproverEmail)
        {
            var referral = await _referralGateway.GetByIdAsync(referralId);

            if (referral is null)
            {
                throw new ArgumentNullException(nameof(referralId), $"Care package not found for: {referralId}");
            }

            var userEmail = _userService.Email;

            if (referral.AssignedBrokerEmail != userEmail)
            {
                throw new UnauthorizedAccessException($"Referral is not assigned to {userEmail}");
            }

            if (referral.Status != ReferralStatus.InProgress)
            {
                throw new InvalidOperationException("Referral is not in a valid state to start editing");
            }

            var approver = await _userGateway.GetByEmailAsync(budgetApproverEmail);

            if (approver is null)
            {
                throw new ArgumentNullException(nameof(budgetApproverEmail), $"Approver not found with: {budgetApproverEmail}");
            }

            var carePackage = await _carePackageGateway.GetByIdAsync(referral.Id);

            if (approver.ApprovalLimit < carePackage.EstimatedYearlyCost)
            {
                throw new UnauthorizedAccessException("Approver does not have high enough approval limit");
            }

            referral.Status = ReferralStatus.AwaitingApproval;
            referral.AssignedApproverEmail = approver.Email;
            var pendingAmendments = referral.ReferralAmendments?.Where(a => a.Status == AmendmentStatus.InProgress);
            if (pendingAmendments != null)
            {
                foreach (var referralAmendment in pendingAmendments)
                {
                    referralAmendment.Status = AmendmentStatus.Resolved;
                }
            }

            await _dbSaver.SaveChangesAsync();

            var metadata = new BudgetApproverAssignmentAuditEventMetadata
            {
                ReferralId = referral.Id,
                AssignedApproverName = approver.Name
            };
            await _auditGateway.AddAuditEvent(AuditEventType.CarePackageBudgetApproverAssigned, referral.SocialCareId, _userService.UserId, metadata);
        }
    }
}
