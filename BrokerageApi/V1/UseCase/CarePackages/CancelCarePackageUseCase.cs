using System;
using System.Threading.Tasks;
using BrokerageApi.V1.Gateways.Interfaces;
using BrokerageApi.V1.Infrastructure;
using BrokerageApi.V1.Services.Interfaces;
using BrokerageApi.V1.UseCase.Interfaces.CarePackageElements;
using BrokerageApi.V1.UseCase.Interfaces.CarePackages;

namespace BrokerageApi.V1.UseCase.CarePackages
{
    public class CancelCarePackageUseCase : ICancelCarePackageUseCase
    {
        private readonly IReferralGateway _referralGateway;
        private readonly ICancelElementUseCase _cancelElementUseCase;
        private readonly IDbSaver _dbSaver;
        private readonly IClockService _clockService;
        public CancelCarePackageUseCase(IReferralGateway referralGateway, ICancelElementUseCase cancelElementUseCase, IDbSaver dbSaver, IClockService clockService)
        {
            _referralGateway = referralGateway;
            _cancelElementUseCase = cancelElementUseCase;
            _dbSaver = dbSaver;
            _clockService = clockService;

        }
        public async Task ExecuteAsync(int referralId)
        {
            var referral = await _referralGateway.GetByIdWithElementsAsync(referralId);

            if (referral is null)
            {
                throw new ArgumentNullException(nameof(referralId), $"Referral not found for: {referralId}");
            }

            foreach (var element in referral.Elements)
            {
                await _cancelElementUseCase.ExecuteAsync(referral.Id, element.Id);
            }

            referral.Status = ReferralStatus.Cancelled;
            referral.UpdatedAt = _clockService.Now;
            await _dbSaver.SaveChangesAsync();
        }
    }
}
