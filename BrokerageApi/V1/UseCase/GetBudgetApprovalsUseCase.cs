using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using BrokerageApi.V1.Gateways.Interfaces;
using BrokerageApi.V1.Infrastructure;
using BrokerageApi.V1.Services.Interfaces;
using BrokerageApi.V1.UseCase.Interfaces;

namespace BrokerageApi.V1.UseCase
{
    public class GetBudgetApprovalsUseCase : IGetBudgetApprovalsUseCase
    {
        private readonly ICarePackageGateway _carePackageGateway;
        private readonly IUserService _userService;
        private readonly IUserGateway _userGateway;

        public GetBudgetApprovalsUseCase(
            ICarePackageGateway carePackageGateway,
            IUserService userService,
            IUserGateway userGateway)
        {
            _carePackageGateway = carePackageGateway;
            _userService = userService;
            _userGateway = userGateway;
        }

        public async Task<IEnumerable<CarePackage>> ExecuteAsync()
        {
            var user = await _userGateway.GetByEmailAsync(_userService.Email);

            if (user.ApprovalLimit is null)
            {
                throw new UnauthorizedAccessException("User has no approval limit set");
            }

            var carePackages = await _carePackageGateway.GetByBudgetApprovalLimitAsync(user.ApprovalLimit.Value);

            return carePackages;
        }
    }
}
