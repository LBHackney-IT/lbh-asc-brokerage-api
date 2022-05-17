using System.Threading.Tasks;
using BrokerageApi.V1.Infrastructure;

namespace BrokerageApi.V1.Gateways.Interfaces
{
    public interface IElementTypeGateway
    {
        public Task<ElementType> GetByIdAsync(int id);
    }
}
