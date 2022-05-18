using System.Threading.Tasks;
using NodaTime;

namespace BrokerageApi.V1.UseCase.Interfaces
{
    public interface IEndElementUseCase
    {
        public Task ExecuteAsync(int id, LocalDate endDate);
    }

}
