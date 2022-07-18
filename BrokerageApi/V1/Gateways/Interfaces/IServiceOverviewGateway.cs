using System.Collections.Generic;
using System.Threading.Tasks;
using BrokerageApi.V1.Infrastructure;

namespace BrokerageApi.V1.Gateways.Interfaces
{
    public interface IServiceOverviewGateway
    {
        public Task<IEnumerable<ServiceOverview>> GetBySocialCareIdAsync(string socialCareId);
    }
}
