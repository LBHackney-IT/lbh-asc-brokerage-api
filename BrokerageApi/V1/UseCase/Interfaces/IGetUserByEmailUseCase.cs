using System.Threading.Tasks;
using BrokerageApi.V1.Infrastructure;

namespace BrokerageApi.V1.UseCase.Interfaces
{
    public interface IGetUserByEmailUseCase
    {
        public Task<User> ExecuteAsync(string email);
    }
}
