using System.Threading.Tasks;
using BrokerageApi.V1.Boundary.Request;
using BrokerageApi.V1.Infrastructure;

namespace BrokerageApi.V1.UseCase.Interfaces
{
    public interface ICreateReferralUseCase
    {
        public Task<Referral> ExecuteAsync(CreateReferralRequest request);
    }
}
