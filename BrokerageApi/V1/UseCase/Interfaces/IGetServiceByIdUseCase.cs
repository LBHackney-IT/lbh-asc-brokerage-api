using System.Threading.Tasks;
using BrokerageApi.V1.Infrastructure;

namespace BrokerageApi.V1.UseCase.Interfaces
{
    public interface IGetServiceByIdUseCase
    {
        public Task<Service> ExecuteAsync(int id);
    }
}
