using System;
using System.Threading.Tasks;
using BrokerageApi.V1.Gateways.Interfaces;
using BrokerageApi.V1.Infrastructure;
using BrokerageApi.V1.Infrastructure.AuditEvents;
using BrokerageApi.V1.Services;
using BrokerageApi.V1.Services.Interfaces;
using BrokerageApi.V1.UseCase.Interfaces;
using NodaTime;

namespace BrokerageApi.V1.UseCase
{
    public class EndElementUseCase : IEndElementUseCase
    {
        private readonly IReferralGateway _referralGateway;
        private readonly IElementGateway _elementGateway;
        private readonly IAuditGateway _auditGateway;
        private readonly IUserService _userService;
        private readonly IDbSaver _dbSaver;
        private readonly IClockService _clockService;

        public EndElementUseCase(
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

        public async Task ExecuteAsync(int referralId, int elementId, LocalDate endDate)
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

            if (element.EndDate != null && element.EndDate < endDate)
            {
                throw new ArgumentException($"Element {element.Id} has an end date before the requested end date");
            }

            element.EndDate = endDate;
            element.UpdatedAt = _clockService.Now;

            await _dbSaver.SaveChangesAsync();

            var metadata = new ElementAuditEventMetadata
            {
                ReferralId = referral.Id,
                ElementId = element.Id,
                ElementDetails = element.Details
            };
            await _auditGateway.AddAuditEvent(AuditEventType.ElementEnded, referral.SocialCareId, _userService.UserId, metadata);
        }
    }
}
