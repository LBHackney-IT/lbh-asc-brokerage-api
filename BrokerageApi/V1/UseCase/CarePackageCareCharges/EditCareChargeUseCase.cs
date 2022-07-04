using System;
using System.Linq;
using System.Threading.Tasks;
using BrokerageApi.V1.Boundary.Request;
using BrokerageApi.V1.Factories;
using BrokerageApi.V1.Gateways.Interfaces;
using BrokerageApi.V1.Infrastructure;
using BrokerageApi.V1.Services.Interfaces;
using BrokerageApi.V1.UseCase.Interfaces.CarePackageCareCharges;

namespace BrokerageApi.V1.UseCase.CarePackageCareCharges
{
    public class EditCareChargeUseCase : IEditCareChargeUseCase
    {
        private readonly IReferralGateway _referralGateway;
        private readonly IElementTypeGateway _elementTypeGateway;
        private readonly IUserService _userService;
        private readonly IClockService _clockService;
        private readonly IDbSaver _dbSaver;

        public EditCareChargeUseCase(
            IReferralGateway referralGateway,
            IElementTypeGateway elementTypeGateway,
            IUserService userService,
            IClockService clockService,
            IDbSaver dbSaver)
        {
            _referralGateway = referralGateway;
            _elementTypeGateway = elementTypeGateway;
            _userService = userService;
            _clockService = clockService;
            _dbSaver = dbSaver;
        }

        public async Task<Element> ExecuteAsync(int referralId, int elementId, EditCareChargeRequest request)
        {
            var referral = await _referralGateway.GetByIdWithElementsAsync(referralId);

            if (referral is null)
            {
                throw new ArgumentNullException(nameof(referralId), $"Referral not found for: {referralId}");
            }

            if (referral.Status != ReferralStatus.Approved)
            {
                throw new InvalidOperationException($"Referral is not in a valid state for editing care charges");
            }

            var element = referral.Elements.SingleOrDefault(e => e.Id == elementId);

            if (element is null)
            {
                throw new ArgumentNullException(nameof(elementId), $"Element not found for: {elementId}");
            }

            var elementType = await _elementTypeGateway.GetByIdAsync(request.ElementTypeId);

            if (elementType is null)
            {
                throw new ArgumentException($"Element type not found for: {request.ElementTypeId}");
            }

            request.ToDatabase(element);
            element.ElementType = elementType;
            element.UpdatedAt = _clockService.Now;
            referral.UpdatedAt = _clockService.Now;

            await _dbSaver.SaveChangesAsync();

            return element;
        }
    }
}
