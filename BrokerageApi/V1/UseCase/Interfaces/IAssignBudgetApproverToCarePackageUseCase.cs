using System.Threading.Tasks;

namespace BrokerageApi.V1.UseCase.Interfaces
{
    public interface IAssignBudgetApproverToCarePackageUseCase
    {
        public Task ExecuteAsync(int referralId, string budgetApproverEmail);
    }

}
