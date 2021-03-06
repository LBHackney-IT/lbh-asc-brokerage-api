using System;
using System.Threading.Tasks;
using BrokerageApi.V1.Gateways.Interfaces;
using BrokerageApi.V1.Infrastructure;
using BrokerageApi.V1.Services.Interfaces;
using BrokerageApi.V1.UseCase.Interfaces.CarePackageElements;

namespace BrokerageApi.V1.UseCase.CarePackageElements
{
    public class DeleteElementUseCase : IDeleteElementUseCase
    {
        private readonly IReferralGateway _referralGateway;
        private readonly IUserService _userService;
        private readonly IDbSaver _dbSaver;
        private readonly IClockService _clockService;

        public DeleteElementUseCase(IReferralGateway referralGateway,
            IUserService userService,
            IDbSaver dbSaver,
            IClockService clockService)
        {
            _referralGateway = referralGateway;
            _userService = userService;
            _dbSaver = dbSaver;
            _clockService = clockService;
        }

        public async Task ExecuteAsync(int referralId, int elementId)
        {
            var referral = await _referralGateway.GetByIdWithElementsAsync(referralId);

            if (referral is null)
            {
                throw new ArgumentNullException(nameof(referralId), $"Referral not found for: {referralId}");
            }

            if (referral.AssignedBrokerEmail != _userService.Email)
            {
                throw new UnauthorizedAccessException($"Referral is not assigned to {_userService.Email}");
            }

            if (referral.Status != ReferralStatus.InProgress)
            {
                throw new InvalidOperationException("Referral is not in a valid state for editing");
            }

            var element = referral.Elements.Find(e => e.Id == elementId);

            if (element is null)
            {
                throw new ArgumentNullException(nameof(elementId), $"Element not found for: {elementId}");
            }

            if (element.ParentElement != null)
            {
                referral.Elements.Add(element.ParentElement);
            }

            if (element.SuspendedElement != null)
            {
                element.SuspendedElement.SuspensionElements.Remove(element);
            }

            referral.Elements.Remove(element);
            referral.UpdatedAt = _clockService.Now;

            await _dbSaver.SaveChangesAsync();
        }
    }
}
