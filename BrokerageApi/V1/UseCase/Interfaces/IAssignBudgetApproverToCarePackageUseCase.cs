using System.Threading.Tasks;
using BrokerageApi.V1.Infrastructure;

namespace BrokerageApi.V1.UseCase.Interfaces
{
    public interface IAssignBudgetApproverToCarePackageUseCase
    {
        public Task ExecuteAsync(int referralId, string budgetApproverEmail);
    }

}
