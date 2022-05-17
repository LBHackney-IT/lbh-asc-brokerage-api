using System.Threading.Tasks;

namespace BrokerageApi.V1.UseCase.Interfaces
{
    public interface IDeleteElementUseCase
    {
        public Task ExecuteAsync(int referralId, int elementId);
    }

}
