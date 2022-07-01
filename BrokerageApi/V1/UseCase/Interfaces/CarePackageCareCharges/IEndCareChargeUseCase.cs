using System.Threading.Tasks;
using NodaTime;

namespace BrokerageApi.V1.UseCase.Interfaces.CarePackageCareCharges
{
    public interface IEndCareChargeUseCase
    {
        public Task ExecuteAsync(int referralId, int elementId, LocalDate endDate);
    }
}
