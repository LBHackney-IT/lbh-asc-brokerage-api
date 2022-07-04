using System.Collections.Generic;
using System.Threading.Tasks;
using BrokerageApi.V1.Infrastructure;

namespace BrokerageApi.V1.Gateways.Interfaces
{
    public interface IProviderGateway
    {
        public Task<IEnumerable<Provider>> FindAsync(string query);
        public Task<Provider> GetByIdAsync(int id);
    }
}
