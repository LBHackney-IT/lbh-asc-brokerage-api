using System.Threading.Tasks;
using BrokerageApi.V1.Infrastructure;

namespace BrokerageApi.V1.UseCase.Interfaces.CarePackages
{
    public interface IStartCarePackageUseCase
    {
        public Task<Referral> ExecuteAsync(int referralId);
    }
}
