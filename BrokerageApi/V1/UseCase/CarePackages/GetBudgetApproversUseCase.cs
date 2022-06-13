using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using BrokerageApi.V1.Gateways.Interfaces;
using BrokerageApi.V1.Infrastructure;
using BrokerageApi.V1.UseCase.Interfaces.CarePackages;

namespace BrokerageApi.V1.UseCase.CarePackages
{
    public class GetBudgetApproversUseCase : IGetBudgetApproversUseCase
    {
        private readonly ICarePackageGateway _carePackageGateway;
        private readonly IUserGateway _userGateway;

        public GetBudgetApproversUseCase(ICarePackageGateway carePackageGateway, IUserGateway userGateway)
        {
            _carePackageGateway = carePackageGateway;
            _userGateway = userGateway;
        }

        public async Task<(IEnumerable<User> approvers, decimal estimatedYearlyCost)> ExecuteAsync(int referralId)
        {
            var carePackage = await _carePackageGateway.GetByIdAsync(referralId);

            if (carePackage is null)
            {
                throw new ArgumentNullException(nameof(referralId), $"Care package not found for: {referralId}");
            }

            if (carePackage.Status != ReferralStatus.InProgress)
            {
                throw new InvalidOperationException("Care package not in correct state");
            }

            var carePackageEstimatedYearlyCost = carePackage.EstimatedYearlyCost;

            return (await _userGateway.GetBudgetApproversAsync(carePackageEstimatedYearlyCost), carePackageEstimatedYearlyCost);
        }
    }
}
