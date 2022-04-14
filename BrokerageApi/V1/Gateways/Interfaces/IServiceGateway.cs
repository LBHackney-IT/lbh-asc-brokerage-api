using System.Collections.Generic;
using System.Threading.Tasks;
using BrokerageApi.V1.Infrastructure;

namespace BrokerageApi.V1.Gateways.Interfaces
{
    public interface IServiceGateway
    {
        public Task<IEnumerable<Service>> GetAllAsync();
    }
}
