using System.Threading.Tasks;

namespace BrokerageApi.V1.UseCase.Interfaces.CarePackages
{
    public interface IApproveCarePackageUseCase
    {
        public Task ExecuteAsync(int referralId);
    }
}
