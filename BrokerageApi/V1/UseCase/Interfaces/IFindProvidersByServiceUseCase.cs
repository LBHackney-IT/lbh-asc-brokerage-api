using System.Collections.Generic;
using System.Threading.Tasks;
using BrokerageApi.V1.Infrastructure;

namespace BrokerageApi.V1.UseCase.Interfaces
{
    public interface IFindProvidersByServiceUseCase
    {
        public Task<IEnumerable<Provider>> ExecuteAsync(Service service, string query);
    }
}
