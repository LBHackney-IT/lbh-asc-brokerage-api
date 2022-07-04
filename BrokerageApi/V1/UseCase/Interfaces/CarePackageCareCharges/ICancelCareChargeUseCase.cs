using System.Threading.Tasks;

namespace BrokerageApi.V1.UseCase.Interfaces.CarePackageCareCharges
{
    public interface ICancelCareChargeUseCase
    {
        public Task ExecuteAsync(int referralId, int elementId, string comment);
    }
}
