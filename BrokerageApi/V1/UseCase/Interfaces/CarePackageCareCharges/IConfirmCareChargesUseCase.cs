using System.Threading.Tasks;

namespace BrokerageApi.V1.UseCase.Interfaces.CarePackageCareCharges
{
    public interface IConfirmCareChargesUseCase
    {
        public Task ExecuteAsync(int referralId);
    }
}
