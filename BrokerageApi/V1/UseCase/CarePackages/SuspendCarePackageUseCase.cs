using System;
using System.Linq;
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
    public class SuspendCarePackageUseCase : ISuspendCarePackageUseCase
    {
        private readonly IReferralGateway _referralGateway;
        private readonly ISuspendElementUseCase _suspendElementUseCase;
        private readonly IDbSaver _dbSaver;
        private readonly IClockService _clockService;
        private readonly IAuditGateway _auditGateway;
        private readonly IUserService _userService;

        public SuspendCarePackageUseCase(
            IReferralGateway referralGateway,
            ISuspendElementUseCase suspendElementUseCase,
            IDbSaver dbSaver,
            IClockService clockService,
            IAuditGateway auditGateway,
            IUserService userService)
        {
            _referralGateway = referralGateway;
            _suspendElementUseCase = suspendElementUseCase;
            _dbSaver = dbSaver;
            _clockService = clockService;
            _auditGateway = auditGateway;
            _userService = userService;
        }

        public async Task ExecuteAsync(int referralId, LocalDate startDate, LocalDate endDate, string comment)
        {
            var referral = await _referralGateway.GetByIdWithElementsAsync(referralId);

            if (referral is null)
            {
                throw new ArgumentNullException(nameof(referralId), $"Referral not found for: {referralId}");
            }

            var elementIds = referral.Elements.Select(e => e.Id).ToArray();
            foreach (var elementId in elementIds)
            {
                await _suspendElementUseCase.ExecuteAsync(referral.Id, elementId, startDate, endDate, null);
            }

            referral.UpdatedAt = _clockService.Now;

            await _dbSaver.SaveChangesAsync();

            var metadata = new CarePackageAuditEventMetadata
            {
                ReferralId = referral.Id,
                Comment = comment
            };
            await _auditGateway.AddAuditEvent(AuditEventType.CarePackageSuspended, referral.SocialCareId, _userService.UserId, metadata);
        }
    }
}
