using System.Threading.Tasks;

namespace BrokerageApi.V1.UseCase.Interfaces.CarePackageElements
{
    public interface ICancelElementUseCase
    {
        public Task ExecuteAsync(int referralId, int elementId);
    }
}
