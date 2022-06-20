using System;
using System.Linq;
using System.Threading.Tasks;
using BrokerageApi.V1.Gateways.Interfaces;
using BrokerageApi.V1.Infrastructure;
using BrokerageApi.V1.UseCase.Interfaces.CarePackageElements;

namespace BrokerageApi.V1.UseCase.CarePackageElements
{
    public class ResetElementUseCase : IResetElementUseCase
    {
        private readonly IReferralGateway _referralGateway;
        private readonly IDeleteElementUseCase _deleteElementUseCase;
        private readonly IDbSaver _dbSaver;

        public ResetElementUseCase(
            IReferralGateway referralGateway,
            IDeleteElementUseCase deleteElementUseCase,
            IDbSaver dbSaver
        )
        {
            _referralGateway = referralGateway;
            _deleteElementUseCase = deleteElementUseCase;
            _dbSaver = dbSaver;
        }

        public async Task ExecuteAsync(int referralId, int elementId)
        {
            var referral = await _referralGateway.GetByIdWithElementsAsync(referralId);

            if (referral is null)
            {
                throw new ArgumentNullException(nameof(referralId), $"Referral not found {referralId}");
            }

            var element = referral.Elements?.SingleOrDefault(e => e.Id == elementId);

            if (element is null)
            {
                throw new ArgumentNullException(nameof(elementId), $"Element not found {elementId}");
            }

            switch (element.InternalStatus)
            {
                case ElementStatus.InProgress:
                    await ResetInProgressElement(element, referral);
                    break;
                case ElementStatus.Approved:
                    await ResetApprovedElement(referral, element);
                    break;
                case ElementStatus.AwaitingApproval:
                case ElementStatus.Inactive:
                case ElementStatus.Active:
                case ElementStatus.Ended:
                case ElementStatus.Suspended:
                case ElementStatus.Cancelled:
                default:
                    throw new InvalidOperationException($"Element {element.Id} is not in a valid state for reset");
            }

        }
        private async Task ResetApprovedElement(Referral referral, Element element)
        {
            var referralElement = referral.ReferralElements.Single(re => re.ElementId == element.Id);
            referralElement.PendingCancellation = null;
            referralElement.PendingEndDate = null;
            referralElement.PendingComment = null;

            if (element.SuspensionElements != null)
            {
                referral.ReferralElements.RemoveAll(re => element.SuspensionElements.Any(e => e.InternalStatus == ElementStatus.InProgress && e.Id == re.ElementId));
                element.SuspensionElements.Clear();
            }

            await _dbSaver.SaveChangesAsync();
        }

        private async Task ResetInProgressElement(Element element, Referral referral)
        {
            if (element.ParentElement != null)
            {
                await _deleteElementUseCase.ExecuteAsync(referral.Id, element.Id);
            }
        }
    }
}
