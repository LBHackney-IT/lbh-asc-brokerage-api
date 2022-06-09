using System;
using System.Threading.Tasks;
using BrokerageApi.V1.Gateways.Interfaces;
using BrokerageApi.V1.Infrastructure;
using BrokerageApi.V1.Services.Interfaces;
using BrokerageApi.V1.UseCase.Interfaces.CarePackages;

namespace BrokerageApi.V1.UseCase.CarePackages
{
    public class StartCarePackageUseCase : IStartCarePackageUseCase
    {
        private readonly IReferralGateway _referralGateway;
        private readonly IUserService _userService;
        private readonly IClockService _clock;
        private readonly IDbSaver _dbSaver;

        public StartCarePackageUseCase(
            IReferralGateway referralGateway,
            IUserService userService,
            IClockService clock,
            IDbSaver dbSaver
        )
        {
            _referralGateway = referralGateway;
            _userService = userService;
            _clock = clock;
            _dbSaver = dbSaver;
        }

        public async Task<Referral> ExecuteAsync(int referralId)
        {
            var referral = await _referralGateway.GetByIdAsync(referralId);

            if (referral is null)
            {
                throw new ArgumentNullException(nameof(referralId), $"Referral not found for: {referralId}");
            }

            if (referral.Status != ReferralStatus.Assigned)
            {
                throw new InvalidOperationException($"Referral is not in a valid state to start editing");
            }

            if (referral.AssignedBroker != _userService.Email)
            {
                throw new UnauthorizedAccessException($"Referral is not assigned to {_userService.Email}");
            }

            referral.Status = ReferralStatus.InProgress;
            referral.StartedAt = _clock.Now;
            await _dbSaver.SaveChangesAsync();

            return referral;
        }
    }
}
