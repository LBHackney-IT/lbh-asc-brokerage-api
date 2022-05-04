using System;
using System.IO;
using System.Threading.Tasks;
using BrokerageApi.V1.Gateways.Interfaces;
using BrokerageApi.V1.Infrastructure;
using BrokerageApi.V1.Services.Interfaces;

namespace BrokerageApi.V1.UseCase.Interfaces
{
    public class DeleteElementUseCase : IDeleteElementUseCase
    {
        private readonly IReferralGateway _referralGateway;
        private readonly IUserService _userService;
        private readonly IDbSaver _dbSaver;
        public DeleteElementUseCase(
            IReferralGateway referralGateway,
            IUserService userService,
            IDbSaver dbSaver
            )
        {
            _referralGateway = referralGateway;
            _userService = userService;
            _dbSaver = dbSaver;
        }

        public async Task ExecuteAsync(int referralId, int elementId)
        {
            var referral = await _referralGateway.GetByIdWithElementsAsync(referralId);

            if (referral is null)
            {
                throw new ArgumentNullException(nameof(referralId), $"Referral not found for: {referralId}");
            }

            if (referral.AssignedTo != _userService.Name)
            {
                throw new UnauthorizedAccessException($"Referral is not assigned to {_userService.Name}");
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

            referral.Elements.Remove(element);

            await _dbSaver.SaveChangesAsync();
        }
    }
}
