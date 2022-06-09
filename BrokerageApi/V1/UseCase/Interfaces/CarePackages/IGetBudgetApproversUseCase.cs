using System.Collections.Generic;
using System.Threading.Tasks;
using BrokerageApi.V1.Infrastructure;

namespace BrokerageApi.V1.UseCase.Interfaces.CarePackages
{
    public interface IGetBudgetApproversUseCase
    {
        public Task<(IEnumerable<User> approvers, decimal estimatedYearlyCost)> ExecuteAsync(int referralId);
    }

}
