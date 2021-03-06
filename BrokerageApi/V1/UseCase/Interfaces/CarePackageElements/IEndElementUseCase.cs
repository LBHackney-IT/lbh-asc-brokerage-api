using System.Threading.Tasks;
using NodaTime;

namespace BrokerageApi.V1.UseCase.Interfaces.CarePackageElements
{
    public interface IEndElementUseCase
    {
        public Task ExecuteAsync(int referralId, int elementId, LocalDate endDate);
    }
}
