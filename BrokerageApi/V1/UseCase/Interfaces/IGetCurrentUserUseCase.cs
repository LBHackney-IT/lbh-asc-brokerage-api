using System.Threading.Tasks;
using BrokerageApi.V1.Infrastructure;

namespace BrokerageApi.V1.UseCase.Interfaces
{
    public interface IGetCurrentUserUseCase
    {
        public Task<User> ExecuteAsync();
    }

}
