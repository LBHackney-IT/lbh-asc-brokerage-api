using System;
using System.Threading.Tasks;
using BrokerageApi.V1.Gateways.Interfaces;
using BrokerageApi.V1.Infrastructure;
using BrokerageApi.V1.Services.Interfaces;
using BrokerageApi.V1.UseCase.Interfaces.CarePackageCareCharges;
using NodaTime;

namespace BrokerageApi.V1.UseCase.CarePackageCareCharges
{
    public class ConfirmCareChargesUseCase : IConfirmCareChargesUseCase
    {
        private readonly IReferralGateway _referralGateway;
        private readonly IClockService _clock;
        private readonly IDbSaver _dbSaver;

        public ConfirmCareChargesUseCase(
            IReferralGateway referralGateway,
            IClockService clockService,
            IDbSaver dbSaver)
        {
            _referralGateway = referralGateway;
            _clock = clockService;
            _dbSaver = dbSaver;
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

            referral.CareChargesConfirmedAt = timeNow;
            referral.UpdatedAt = timeNow;

            await _dbSaver.SaveChangesAsync();
        }

        private static bool IsInProgressCareCharge(Element element)
        {
            return element.InternalStatus == ElementStatus.InProgress;
        }
    }
}
