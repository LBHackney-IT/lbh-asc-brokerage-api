using System.Threading.Tasks;
using BrokerageApi.V1.Infrastructure;
using BrokerageApi.V1.UseCase.Interfaces;

namespace BrokerageApi.V1.UseCase
{
    public class GetCurrentUserUseCase : IGetCurrentUserUseCase
    {
        public Task<User> ExecuteAsync()
        {
            throw new System.NotImplementedException();
        }
    }
}
