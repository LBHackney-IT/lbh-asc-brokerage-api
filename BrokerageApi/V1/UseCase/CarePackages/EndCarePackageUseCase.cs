using System;
using System.Threading.Tasks;
using BrokerageApi.V1.Gateways.Interfaces;
using BrokerageApi.V1.Infrastructure;
using BrokerageApi.V1.Infrastructure.AuditEvents;
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
        private readonly IAuditGateway _auditGateway;
        private readonly IUserService _userService;
        public EndCarePackageUseCase(
            IReferralGateway referralGateway,
            IEndElementUseCase endElementUseCase,
            IDbSaver dbSaver,
            IClockService clockService,
            IAuditGateway auditGateway,
            IUserService userService)
        {
            _referralGateway = referralGateway;
            _endElementUseCase = endElementUseCase;
            _dbSaver = dbSaver;
            _clockService = clockService;
            _auditGateway = auditGateway;
            _userService = userService;
        }

        public async Task ExecuteAsync(int referralId, LocalDate endDate, string comment)
        {
            var referral = await _referralGateway.GetByIdWithElementsAsync(referralId);

            if (referral is null)
            {
                throw new ArgumentNullException(nameof(referralId), $"Referral not found for: {referralId}");
            }

            foreach (var element in referral.Elements)
            {
                await _endElementUseCase.ExecuteAsync(referral.Id, element.Id, endDate, null);
            }

            referral.UpdatedAt = _clockService.Now;
            referral.Comment = comment;
            await _dbSaver.SaveChangesAsync();

            var metadata = new ReferralAuditEventMetadata
            {
                ReferralId = referral.Id,
                Comment = comment
            };
            await _auditGateway.AddAuditEvent(AuditEventType.CarePackageEnded, referral.SocialCareId, _userService.UserId, metadata);
        }
    }
}
