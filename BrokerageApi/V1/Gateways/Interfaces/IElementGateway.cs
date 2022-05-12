using System.Collections.Generic;
using System.Threading.Tasks;
using BrokerageApi.V1.Infrastructure;

namespace BrokerageApi.V1.Gateways.Interfaces
{
    public interface IElementGateway
    {
        public Task<IEnumerable<Element>> GetCurrentAsync();
        public Task<Element> GetByIdAsync(int id);
    }
}
