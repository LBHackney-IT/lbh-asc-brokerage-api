using System.Threading.Tasks;

namespace BrokerageApi.V1.UseCase.Interfaces.CarePackages
{
    public interface ICancelCarePackageUseCase
    {
        public Task ExecuteAsync(int referralId);
    }

}
