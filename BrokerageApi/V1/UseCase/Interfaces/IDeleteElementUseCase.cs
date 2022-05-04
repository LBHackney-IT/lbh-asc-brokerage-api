using System.Threading.Tasks;
using BrokerageApi.V1.Infrastructure;

namespace BrokerageApi.V1.UseCase.Interfaces
{
    public interface IDeleteElementUseCase
    {
        public Task ExecuteAsync(int referralId, int elementId);
    }

}
