using System.Threading.Tasks;
using NodaTime;

namespace BrokerageApi.V1.UseCase.Interfaces
{
    public interface ICancelElementUseCase
    {
        public Task ExecuteAsync(int referralId, int elementId, LocalDate endDate);
    }
}
