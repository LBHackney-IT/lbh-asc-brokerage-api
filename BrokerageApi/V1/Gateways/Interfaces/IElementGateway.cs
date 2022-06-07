using System.Collections.Generic;
using System.Threading.Tasks;
using BrokerageApi.V1.Infrastructure;

namespace BrokerageApi.V1.Gateways.Interfaces
{
    public interface IElementGateway
    {
        public Task<IEnumerable<Element>> GetCurrentAsync();

        public Task<IEnumerable<Element>> GetBySocialCareId(string socialCareId);

        public Task<Element> GetByIdAsync(int id);

        public Task AddElementAsync(Element element);
    }
}
