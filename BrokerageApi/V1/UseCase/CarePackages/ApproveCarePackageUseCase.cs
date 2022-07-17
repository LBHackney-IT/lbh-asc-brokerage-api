using System;
using System.Linq;
using System.Threading.Tasks;
using BrokerageApi.V1.Gateways.Interfaces;
using BrokerageApi.V1.Infrastructure;
using BrokerageApi.V1.Infrastructure.AuditEvents;
using BrokerageApi.V1.Services.Interfaces;
using BrokerageApi.V1.UseCase.Interfaces.CarePackages;
using NodaTime;

namespace BrokerageApi.V1.UseCase.CarePackages
{
    public class ApproveCarePackageUseCase : IApproveCarePackageUseCase
    {
        private readonly ICarePackageGateway _carePackageGateway;
        private readonly IReferralGateway _referralGateway;
        private readonly IUserService _userService;
        private readonly IUserGateway _userGateway;
        private readonly IDbSaver _dbSaver;
        private readonly IAuditGateway _auditGateway;
        private readonly IClockService _clock;

        public ApproveCarePackageUseCase(ICarePackageGateway carePackageGateway,
            IReferralGateway referralGateway,
            IUserService userService,
            IUserGateway userGateway,
            IDbSaver dbSaver,
            IAuditGateway auditGateway,
            IClockService clock)
        {
            _carePackageGateway = carePackageGateway;
            _referralGateway = referralGateway;
            _userService = userService;
            _userGateway = userGateway;
            _dbSaver = dbSaver;
            _auditGateway = auditGateway;
            _clock = clock;
        }

        public async Task ExecuteAsync(int referralId)
        {
            var referral = await _referralGateway.GetByIdWithElementsAsync(referralId);

            if (referral is null)
            {
                throw new ArgumentNullException(nameof(referralId), $"Referral not found for: {referralId}");
            }

            var carePackage = await _carePackageGateway.GetByIdAsync(referral.Id);

            if (carePackage is null)
            {
                throw new ArgumentNullException(nameof(referralId), $"Care package not found for: {referralId}");
            }

            if (carePackage.Status != ReferralStatus.AwaitingApproval)
            {
                throw new InvalidOperationException("Referral is not in a valid state for approval");
            }

            var user = await _userGateway.GetByEmailAsync(_userService.Email);

            if (carePackage.EstimatedYearlyCost > user.ApprovalLimit)
            {
                throw new UnauthorizedAccessException("Approver does not have high enough approval limit");
            }

            var existingReferrals = await _referralGateway.GetBySocialCareIdWithElementsAsync(referral.SocialCareId);
            var sixMonthsAgo = _clock.Now - Duration.FromDays(180);

            foreach (var oldReferral in existingReferrals.Where(r => r.Status == ReferralStatus.Approved))
            {
                oldReferral.Status = ReferralStatus.Ended;

                if (oldReferral.CareChargesConfirmedAt > sixMonthsAgo)
                {
                    referral.CareChargeStatus = CareChargeStatus.Existing;
                }
            }

            referral.Status = ReferralStatus.Approved;

            if (referral.Elements != null)
            {
                foreach (var e in referral.Elements)
                {
                    e.InternalStatus = ElementStatus.Approved;

                    if (e.ParentElement != null)
                    {
                        e.ParentElement.EndDate = e.StartDate.PlusDays(-1);
                    }

                    await ApplyPendingStates(e, referral);
                }

                if (referral.IsCancelled)
                {
                    referral.CareChargeStatus = CareChargeStatus.Cancellation;
                }

                if (referral.IsEnded)
                {
                    referral.CareChargeStatus = CareChargeStatus.Termination;
                }

                if (referral.IsSuspended)
                {
                    referral.CareChargeStatus = CareChargeStatus.Suspension;
                }

                if (referral.ServiceElements.Any(e => e.IsResidential))
                {
                    referral.IsResidential = true;
                }
            }

            await _dbSaver.SaveChangesAsync();

            var metadata = new CarePackageApprovalAuditEventMetadata
            {
                ReferralId = referral.Id,
            };
            await _auditGateway.AddAuditEvent(AuditEventType.CarePackageApproved, referral.SocialCareId, _userService.UserId, metadata);
        }

        private async Task ApplyPendingStates(Element e, Referral referral)
        {
            var referralElement = e.ReferralElements?.SingleOrDefault(re => re.ReferralId == referral.Id);

            if (referralElement is null) return;

            var metadata = new ElementAuditEventMetadata
            {
                ReferralId = referral.Id,
                ElementId = e.Id,
                ElementDetails = e.Details,
                Comment = referralElement.PendingComment
            };

            if (referralElement.PendingComment != null)
            {
                e.Comment = referralElement.PendingComment;
                referralElement.PendingComment = null;
            }

            if (referralElement.PendingEndDate != null)
            {
                e.EndDate = referralElement.PendingEndDate;
                referralElement.PendingEndDate = null;
                await _auditGateway.AddAuditEvent(AuditEventType.ElementEnded, referral.SocialCareId, _userService.UserId, metadata);
            }

            if (referralElement.PendingCancellation)
            {
                e.InternalStatus = ElementStatus.Cancelled;
                referralElement.PendingCancellation = false;
                await _auditGateway.AddAuditEvent(AuditEventType.ElementCancelled, referral.SocialCareId, _userService.UserId, metadata);
            }
        }
    }
}
