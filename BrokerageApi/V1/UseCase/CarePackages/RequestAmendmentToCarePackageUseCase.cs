using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using BrokerageApi.V1.Gateways.Interfaces;
using BrokerageApi.V1.Infrastructure;
using BrokerageApi.V1.Infrastructure.AuditEvents;
using BrokerageApi.V1.Services.Interfaces;
using BrokerageApi.V1.UseCase.Interfaces.CarePackages;

namespace BrokerageApi.V1.UseCase.CarePackages
{
    public class RequestAmendmentToCarePackageUseCase : IRequestAmendmentToCarePackageUseCase
    {
        private readonly ICarePackageGateway _carePackageGateway;
        private readonly IReferralGateway _referralGateway;
        private readonly IUserService _userService;
        private readonly IUserGateway _userGateway;
        private readonly IDbSaver _dbSaver;
        private readonly IAuditGateway _auditGateway;
        private readonly IClockService _clockService;

        public RequestAmendmentToCarePackageUseCase(ICarePackageGateway carePackageGateway,
            IReferralGateway referralGateway,
            IUserService userService,
            IUserGateway userGateway,
            IDbSaver dbSaver,
            IAuditGateway auditGateway,
            IClockService clockService)
        {
            _carePackageGateway = carePackageGateway;
            _referralGateway = referralGateway;
            _userService = userService;
            _userGateway = userGateway;
            _dbSaver = dbSaver;
            _auditGateway = auditGateway;
            _clockService = clockService;
        }

        public async Task ExecuteAsync(int referralId, string comment)
        {
            var referral = await _referralGateway.GetByIdAsync(referralId);

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

            referral.Status = ReferralStatus.InProgress;

            var amendment = new ReferralAmendment
            {
                Status = AmendmentStatus.InProgress,
                Comment = comment,
                RequestedAt = _clockService.Now
            };

            referral.ReferralAmendments ??= new List<ReferralAmendment>();
            referral.ReferralAmendments.Add(amendment);

            await _dbSaver.SaveChangesAsync();

            var metadata = new ReferralAuditEventMetadata()
            {
                ReferralId = referral.Id,
                Comment = comment
            };
            await _auditGateway.AddAuditEvent(AuditEventType.AmendmentRequested, referral.SocialCareId, _userService.UserId, metadata);
        }
    }
}
