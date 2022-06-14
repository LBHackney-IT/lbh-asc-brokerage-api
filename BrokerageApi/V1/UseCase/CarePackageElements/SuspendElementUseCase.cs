using System;
using System.Linq;
using System.Threading.Tasks;
using BrokerageApi.V1.Gateways.Interfaces;
using BrokerageApi.V1.Infrastructure;
using BrokerageApi.V1.Infrastructure.AuditEvents;
using BrokerageApi.V1.Services.Interfaces;
using BrokerageApi.V1.UseCase.Interfaces.CarePackageElements;
using NodaTime;

namespace BrokerageApi.V1.UseCase.CarePackageElements
{
    public class SuspendElementUseCase : ISuspendElementUseCase
    {
        private readonly IReferralGateway _referralGateway;
        private readonly IAuditGateway _auditGateway;
        private readonly IUserService _userService;
        private readonly IDbSaver _dbSaver;
        private readonly IClockService _clockService;
        private readonly IElementGateway _elementGateway;

        public SuspendElementUseCase(
            IReferralGateway referralGateway,
            IAuditGateway auditGateway,
            IUserService userService,
            IDbSaver dbSaver,
            IClockService clockService,
            IElementGateway elementGateway
        )
        {
            _referralGateway = referralGateway;
            _auditGateway = auditGateway;
            _userService = userService;
            _dbSaver = dbSaver;
            _clockService = clockService;
            _elementGateway = elementGateway;
        }

        public async Task ExecuteAsync(int referralId, int elementId, LocalDate startDate, LocalDate? endDate, string comment)
        {
            var referral = await _referralGateway.GetByIdWithElementsAsync(referralId);

            if (referral is null)
            {
                throw new ArgumentNullException(nameof(referralId), $"Referral not found {referralId}");
            }

            var element = referral.Elements.SingleOrDefault(e => e.Id == elementId);

            if (element is null)
            {
                throw new ArgumentNullException(nameof(elementId), $"Element not found {elementId}");
            }

            if (element.InternalStatus != ElementStatus.Approved)
            {
                throw new InvalidOperationException($"Element {element.Id} is not approved");
            }

            if (startDate < element.StartDate || (element.EndDate != null && endDate > element.EndDate))
            {
                throw new ArgumentException("Requested dates do not fall in elements dates");
            }

            var newElement = new Element(element)
            {
                SuspendedElementId = element.Id,
                CreatedAt = _clockService.Now,
                UpdatedAt = _clockService.Now,
                InternalStatus = ElementStatus.InProgress,
                StartDate = startDate,
                EndDate = endDate,
                IsSuspension = true,
                Comment = comment,
                CreatedBy = _userService.Email
            };
            element.Comment = comment;

            await _elementGateway.AddElementAsync(newElement);

            await _dbSaver.SaveChangesAsync();

            var metadata = new ElementAuditEventMetadata
            {
                ReferralId = referral.Id,
                ElementId = element.Id,
                ElementDetails = element.Details,
                Comment = comment
            };
            await _auditGateway.AddAuditEvent(AuditEventType.ElementSuspended, referral.SocialCareId, _userService.UserId, metadata);
        }
    }
}
