using System;
using System.Threading.Tasks;
using BrokerageApi.V1.Gateways.Interfaces;
using BrokerageApi.V1.Infrastructure;
using BrokerageApi.V1.Infrastructure.AuditEvents;
using BrokerageApi.V1.Services.Interfaces;
using BrokerageApi.V1.UseCase.Interfaces;

namespace BrokerageApi.V1.UseCase
{
    public class ArchiveReferralUseCase : IArchiveReferralUseCase
    {
        private readonly IReferralGateway _referralGateway;
        private readonly IDbSaver _dbSaver;
        private readonly IAuditGateway _auditGateway;
        private readonly IUserService _userService;

        public ArchiveReferralUseCase(IReferralGateway referralGateway, IDbSaver dbSaver, IAuditGateway auditGateway, IUserService userService)
        {
            _referralGateway = referralGateway;
            _dbSaver = dbSaver;
            _auditGateway = auditGateway;
            _userService = userService;
        }

        public async Task ExecuteAsync(int referralId, string comment)
        {
            var referral = await _referralGateway.GetByIdAsync(referralId);

            if (referral is null)
            {
                throw new ArgumentNullException(nameof(referralId), $"Referral not found for: {referralId}");
            }

            if (!(referral.Status == ReferralStatus.InProgress || referral.Status == ReferralStatus.Assigned || referral.Status == ReferralStatus.Unassigned))
            {
                throw new InvalidOperationException("Referral is not in a valid state for archive");
            }

            referral.Status = ReferralStatus.Archived;
            referral.Comment = comment;
            await _dbSaver.SaveChangesAsync();

            var metadata = new ReferralAuditEventMetadata()
            {
                ReferralId = referral.Id,
                Comment = comment
            };
            await _auditGateway.AddAuditEvent(AuditEventType.ReferralArchived, referral.SocialCareId, _userService.UserId, metadata);
        }
    }
}
