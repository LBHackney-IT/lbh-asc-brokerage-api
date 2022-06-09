using System;
using System.Linq;
using System.Threading.Tasks;
using BrokerageApi.V1.Gateways.Interfaces;
using BrokerageApi.V1.Infrastructure;
using BrokerageApi.V1.Infrastructure.AuditEvents;
using BrokerageApi.V1.Services.Interfaces;
using BrokerageApi.V1.UseCase.Interfaces.CarePackageElements;

namespace BrokerageApi.V1.UseCase.CarePackageElements
{
    public class CancelElementUseCase : ICancelElementUseCase
    {
        private readonly IReferralGateway _referralGateway;
        private readonly IElementGateway _elementGateway;
        private readonly IAuditGateway _auditGateway;
        private readonly IUserService _userService;
        private readonly IDbSaver _dbSaver;
        private readonly IClockService _clockService;

        public CancelElementUseCase(
            IReferralGateway referralGateway,
            IElementGateway elementGateway,
            IAuditGateway auditGateway,
            IUserService userService,
            IDbSaver dbSaver,
            IClockService clockService
        )
        {
            _referralGateway = referralGateway;
            _elementGateway = elementGateway;
            _auditGateway = auditGateway;
            _userService = userService;
            _dbSaver = dbSaver;
            _clockService = clockService;
        }

        public async Task ExecuteAsync(int referralId, int elementId, string comment)
        {
            var referral = await _referralGateway.GetByIdAsync(referralId);

            if (referral is null)
            {
                throw new ArgumentNullException(nameof(referralId), $"Referral not found {referralId}");
            }

            var element = await _elementGateway.GetByIdAsync(elementId);

            if (element is null)
            {
                throw new ArgumentNullException(nameof(elementId), $"Element not found {elementId}");
            }

            if (element.InternalStatus != ElementStatus.Approved)
            {
                throw new InvalidOperationException($"Element {element.Id} is not approved");
            }

            var referralElement = element.ReferralElements.Single(re => re.ElementId == element.Id);
            referralElement.PendingCancellation = true;
            referralElement.PendingComment = comment;

            await _dbSaver.SaveChangesAsync();

            var metadata = new ElementAuditEventMetadata
            {
                ReferralId = referral.Id,
                ElementId = element.Id,
                ElementDetails = element.Details,
                Comment = comment
            };
            await _auditGateway.AddAuditEvent(AuditEventType.ElementCancelled, referral.SocialCareId, _userService.UserId, metadata);
        }
    }
}
