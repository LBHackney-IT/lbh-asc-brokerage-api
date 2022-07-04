using System;
using System.Collections.Generic;
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
    public class CreateCareChargeUseCase : ICreateCareChargeUseCase
    {
        private readonly IReferralGateway _referralGateway;
        private readonly IElementTypeGateway _elementTypeGateway;
        private readonly IProviderGateway _providerGateway;
        private readonly IUserService _userService;
        private readonly IClockService _clock;
        private readonly IDbSaver _dbSaver;

        public CreateCareChargeUseCase(
            IReferralGateway referralGateway,
            IElementTypeGateway elementTypeGateway,
            IProviderGateway providerGateway,
            IUserService userService,
            IClockService clock,
            IDbSaver dbSaver
        )
        {
            _referralGateway = referralGateway;
            _elementTypeGateway = elementTypeGateway;
            _providerGateway = providerGateway;
            _userService = userService;
            _clock = clock;
            _dbSaver = dbSaver;
        }

        public async Task<Element> ExecuteAsync(int referralId, CreateCareChargeRequest request)
        {
            var referral = await _referralGateway.GetByIdWithElementsAsync(referralId);

            if (referral is null)
            {
                throw new ArgumentNullException(nameof(referralId), $"Referral not found for: {referralId}");
            }

            if (referral.Status != ReferralStatus.Approved)
            {
                throw new InvalidOperationException($"Referral is not in a valid state for adding care charges");
            }

            var elementType = await _elementTypeGateway.GetByIdAsync(request.ElementTypeId);

            if (elementType is null)
            {
                throw new ArgumentException($"Element type not found for: {request.ElementTypeId}");
            }

            var timeNow = _clock.Now;
            var element = request.ToDatabase();
            element.ElementType = elementType;
            element.SocialCareId = referral.SocialCareId;
            element.CreatedAt = timeNow;
            element.CreatedBy = _userService.Email;

            referral.Elements ??= new List<Element>();
            referral.Elements.Add(element);

            if (request.ParentElementId != null)
            {
                referral.Elements.Remove(referral.Elements.Single(e => e.Id == request.ParentElementId));
            }

            referral.UpdatedAt = timeNow;

            await _dbSaver.SaveChangesAsync();

            return element;
        }
    }
}
