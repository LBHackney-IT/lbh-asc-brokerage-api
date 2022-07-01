using System.Threading.Tasks;

namespace BrokerageApi.V1.UseCase.Interfaces.CarePackageCareCharges
{
    public interface IResetCareChargeUseCase
    {
        public Task ExecuteAsync(int referralId, int elementId);
    }
}
