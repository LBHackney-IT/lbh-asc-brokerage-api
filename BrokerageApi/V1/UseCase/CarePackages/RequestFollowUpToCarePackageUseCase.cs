using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using BrokerageApi.V1.Gateways.Interfaces;
using BrokerageApi.V1.Infrastructure;
using BrokerageApi.V1.Infrastructure.AuditEvents;
using BrokerageApi.V1.Services.Interfaces;
using BrokerageApi.V1.UseCase.Interfaces.CarePackages;
using NodaTime;

namespace BrokerageApi.V1.UseCase.CarePackages
{
    public class RequestFollowUpToCarePackageUseCase : IRequestFollowUpToCarePackageUseCase
    {
        private readonly ICarePackageGateway _carePackageGateway;
        private readonly IReferralGateway _referralGateway;
        private readonly IUserService _userService;
        private readonly IDbSaver _dbSaver;
        private readonly IAuditGateway _auditGateway;
        private readonly IClockService _clockService;

        public RequestFollowUpToCarePackageUseCase(ICarePackageGateway carePackageGateway,
            IReferralGateway referralGateway,
            IUserService userService,
            IDbSaver dbSaver,
            IAuditGateway auditGateway,
            IClockService clockService)
        {
            _carePackageGateway = carePackageGateway;
            _referralGateway = referralGateway;
            _userService = userService;
            _dbSaver = dbSaver;
            _auditGateway = auditGateway;
            _clockService = clockService;
        }

        public async Task ExecuteAsync(int referralId, string comment, LocalDate followUpDate)
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

            if (carePackage.Status != ReferralStatus.Approved)
            {
                throw new InvalidOperationException("Referral is not in a valid state for requesting a follow-up");
            }

            var followUp = new ReferralFollowUp
            {
                Comment = comment,
                Date = followUpDate,
                Status = FollowUpStatus.InProgress,
                RequestedAt = _clockService.Now,
                RequestedByEmail = _userService.Email
            };

            referral.ReferralFollowUps ??= new List<ReferralFollowUp>();
            referral.ReferralFollowUps.Add(followUp);

            await _dbSaver.SaveChangesAsync();

            var metadata = new ReferralFollowUpAuditEventMetadata()
            {
                ReferralId = referral.Id,
                Comment = comment,
                Date = followUpDate
            };
            await _auditGateway.AddAuditEvent(AuditEventType.FollowUpRequested, referral.SocialCareId, _userService.UserId, metadata);
        }
    }
}
