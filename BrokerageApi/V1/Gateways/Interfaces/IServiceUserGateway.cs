using System.Collections.Generic;
using System.Threading.Tasks;
using BrokerageApi.V1.Boundary.Request;
using BrokerageApi.V1.Infrastructure;

namespace BrokerageApi.V1.Gateways.Interfaces
{
    public interface IServiceUserGateway
    {
        public Task<IEnumerable<ServiceUser>> GetByRequestAsync(GetServiceUserRequest request);
    }
}
