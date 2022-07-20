using System;
using System.Threading.Tasks;
using BrokerageApi.V1.Gateways.Interfaces;
using BrokerageApi.V1.Infrastructure;
using BrokerageApi.V1.Services.Interfaces;
using BrokerageApi.V1.UseCase.Interfaces.CarePackages;
using Npgsql;

namespace BrokerageApi.V1.UseCase.CarePackages
{
    public class StartCarePackageUseCase : IStartCarePackageUseCase
    {
        private readonly IReferralGateway _referralGateway;
        private readonly IElementGateway _elementGateway;
        private readonly IUserService _userService;
        private readonly IClockService _clock;
        private readonly IDbSaver _dbSaver;

        public StartCarePackageUseCase(
            IReferralGateway referralGateway,
            IElementGateway elementGateway,
            IUserService userService,
            IClockService clock,
            IDbSaver dbSaver
        )
        {
            _referralGateway = referralGateway;
            _elementGateway = elementGateway;
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

            if (!CanBeStarted(referral.Status))
            {
                throw new InvalidOperationException($"Referral is not in a valid state to start editing");
            }

            if (referral.AssignedBrokerEmail != _userService.Email)
            {
                throw new UnauthorizedAccessException($"Referral is not assigned to {_userService.Email}");
            }

            if (referral.Status == ReferralStatus.Assigned)
            {
                var elements = await _elementGateway.GetCurrentBySocialCareId(referral.SocialCareId);

                referral.ReferralElements.Clear();

                foreach (var element in elements)
                {
                    var referralElement = new ReferralElement() { ReferralId = referral.Id, ElementId = element.Id };
                    referral.ReferralElements.Add(referralElement);
                }

                referral.Status = ReferralStatus.InProgress;
                referral.StartedAt = _clock.Now;

                try
                {
                    await _dbSaver.SaveChangesAsync();
                }
                catch (Microsoft.EntityFrameworkCore.DbUpdateException ex)
                {
                    PostgresException innerEx = ex.InnerException as PostgresException;

                    if (innerEx?.SqlState == PostgresErrorCodes.UniqueViolation && innerEx.ConstraintName == "ix_referrals_social_care_id")
                    {
                        throw new InvalidOperationException($"A referral for {referral.ResidentName} is already in progress or awaiting approval");
                    }
                    else
                    {
                        throw;
                    }
                }
            }

            return referral;
        }

        private static bool CanBeStarted(ReferralStatus status)
        {
            return status == ReferralStatus.Assigned || status == ReferralStatus.InProgress;
        }
    }
}
