using System.Threading.Tasks;
using BrokerageApi.V1.Boundary.Request;
using BrokerageApi.V1.Infrastructure;

namespace BrokerageApi.V1.UseCase.Interfaces
{
    public interface IAssignBrokerToReferralUseCase
    {
        public Task<Referral> ExecuteAsync(int referralId, AssignBrokerRequest request);
    }
}
