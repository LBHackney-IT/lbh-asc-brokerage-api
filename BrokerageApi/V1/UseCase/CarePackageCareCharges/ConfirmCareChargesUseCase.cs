using System;
using System.Threading.Tasks;
using BrokerageApi.V1.Gateways.Interfaces;
using BrokerageApi.V1.Infrastructure;
using BrokerageApi.V1.Infrastructure.AuditEvents;
using BrokerageApi.V1.Services.Interfaces;
using BrokerageApi.V1.UseCase.Interfaces.CarePackageCareCharges;

namespace BrokerageApi.V1.UseCase.CarePackageCareCharges
{
    public class ConfirmCareChargesUseCase : IConfirmCareChargesUseCase
    {
        private readonly IReferralGateway _referralGateway;
        private readonly IClockService _clock;
        private readonly IUserService _userService;
        private readonly IDbSaver _dbSaver;
        private readonly IAuditGateway _auditGateway;

        public ConfirmCareChargesUseCase(
            IReferralGateway referralGateway,
            IClockService clockService,
            IUserService userService,
            IDbSaver dbSaver,
            IAuditGateway auditGateway
        )
        {
            _referralGateway = referralGateway;
            _clock = clockService;
            _userService = userService;
            _dbSaver = dbSaver;
            _auditGateway = auditGateway;
        }

        public async Task ExecuteAsync(int referralId)
        {
            var referral = await _referralGateway.GetByIdWithElementsAsync(referralId);

            if (referral is null)
            {
                throw new ArgumentNullException(nameof(referralId), $"Referral not found for: {referralId}");
            }

            if (referral.Status != ReferralStatus.Approved)
            {
                throw new InvalidOperationException("Referral is not in a valid state for confirming care charges");
            }

            if (referral.CareChargesConfirmedAt != null)
            {
                throw new InvalidOperationException("Charges have already been confirmed for this care package");
            }

            var timeNow = _clock.Now;

            if (referral.Elements != null)
            {
                foreach (var element in referral.Elements)
                {
                    if (IsInProgressCareCharge(element))
                    {
                        element.InternalStatus = ElementStatus.Approved;
                        element.UpdatedAt = timeNow;
                    }
                }
            }

            if (referral.ReferralFollowUps != null)
            {
                foreach (var followUp in referral.ReferralFollowUps)
                {
                    followUp.Status = FollowUpStatus.Resolved;
                }
            }

            referral.CareChargesConfirmedAt = timeNow;
            referral.UpdatedAt = timeNow;

            await _dbSaver.SaveChangesAsync();

            var metadata = new CareChargesConfirmedAuditEventMetadata
            {
                ReferralId = referral.Id,
            };

            await _auditGateway.AddAuditEvent(AuditEventType.CareChargesConfirmed, referral.SocialCareId, _userService.UserId, metadata);
        }

        private static bool IsInProgressCareCharge(Element element)
        {
            return element.InternalStatus == ElementStatus.InProgress;
        }
    }
}
