using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using BrokerageApi.V1.Boundary.Request;
using BrokerageApi.V1.Factories;
using BrokerageApi.V1.Gateways.Interfaces;
using BrokerageApi.V1.Infrastructure;
using BrokerageApi.V1.Services.Interfaces;
using BrokerageApi.V1.UseCase.Interfaces;
using Npgsql;

namespace BrokerageApi.V1.UseCase
{
    public class CreateElementUseCase : ICreateElementUseCase
    {
        private readonly IReferralGateway _referralGateway;
        private readonly IElementTypeGateway _elementTypeGateway;
        private readonly IProviderGateway _providerGateway;
        private readonly IUserService _userService;
        private readonly IDbSaver _dbSaver;

        public CreateElementUseCase(
            IReferralGateway referralGateway,
            IElementTypeGateway elementTypeGateway,
            IProviderGateway providerGateway,
            IUserService userService,
            IDbSaver dbSaver
        )
        {
            _referralGateway = referralGateway;
            _elementTypeGateway = elementTypeGateway;
            _providerGateway = providerGateway;
            _userService = userService;
            _dbSaver = dbSaver;
        }

        public async Task<Element> ExecuteAsync(int referralId, CreateElementRequest request)
        {
            var referral = await _referralGateway.GetByIdAsync(referralId);

            if (referral is null)
            {
                throw new ArgumentNullException(nameof(referralId), $"Referral not found for: {referralId}");
            }

            if (referral.Status != ReferralStatus.InProgress)
            {
                throw new InvalidOperationException($"Referral is not in a valid state for editing");
            }

            if (referral.AssignedTo != _userService.Name)
            {
                throw new UnauthorizedAccessException($"Referral is not assigned to {_userService.Name}");
            }

            var elementType = await _elementTypeGateway.GetByIdAsync(request.ElementTypeId);

            if (elementType is null)
            {
                throw new ArgumentException($"Element type not found for: {request.ElementTypeId}");
            }

            var provider = await _providerGateway.GetByIdAsync(request.ProviderId);

            if (provider is null)
            {
                throw new ArgumentException($"Provider not found for: {request.ProviderId}");
            }

            var element = request.ToDatabase();
            element.ElementType = elementType;
            element.Provider = provider;
            element.SocialCareId = referral.SocialCareId;

            referral.Elements ??= new List<Element>();
            referral.Elements.Add(element);

            await _dbSaver.SaveChangesAsync();

            return element;
        }
    }
}
