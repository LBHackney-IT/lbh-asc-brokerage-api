using System;
using System.Threading.Tasks;
using BrokerageApi.V1.Gateways.Interfaces;
using BrokerageApi.V1.Infrastructure;
using BrokerageApi.V1.Services.Interfaces;
using BrokerageApi.V1.UseCase.Interfaces.CarePackageElements;
using BrokerageApi.V1.UseCase.Interfaces.CarePackages;
using NodaTime;

namespace BrokerageApi.V1.UseCase.CarePackages
{
    public class EndCarePackageUseCase : IEndCarePackageUseCase
    {
        private readonly IReferralGateway _referralGateway;
        private readonly IEndElementUseCase _endElementUseCase;
        private readonly IDbSaver _dbSaver;
        private readonly IClockService _clockService;
        public EndCarePackageUseCase(IReferralGateway referralGateway, IEndElementUseCase endElementUseCase, IDbSaver dbSaver, IClockService clockService)
        {
            _referralGateway = referralGateway;
            _endElementUseCase = endElementUseCase;
            _dbSaver = dbSaver;
            _clockService = clockService;
        }

        public async Task ExecuteAsync(int referralId, LocalDate endDate)
        {
            var referral = await _referralGateway.GetByIdWithElementsAsync(referralId);

            if (referral is null)
            {
                throw new ArgumentNullException(nameof(referralId), $"Referral not found for: {referralId}");
            }

            foreach (var element in referral.Elements)
            {
                await _endElementUseCase.ExecuteAsync(referral.Id, element.Id, endDate);
            }

            referral.UpdatedAt = _clockService.Now;
            await _dbSaver.SaveChangesAsync();
        }
    }
}
