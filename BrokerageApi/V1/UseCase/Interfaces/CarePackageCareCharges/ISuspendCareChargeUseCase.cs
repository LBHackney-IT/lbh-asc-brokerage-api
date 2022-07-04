using System.Threading.Tasks;
using NodaTime;

namespace BrokerageApi.V1.UseCase.Interfaces.CarePackageCareCharges
{
    public interface ISuspendCareChargeUseCase
    {
        public Task ExecuteAsync(int referralId, int elementId, LocalDate startDate, LocalDate? endDate, string comment);
    }
}
