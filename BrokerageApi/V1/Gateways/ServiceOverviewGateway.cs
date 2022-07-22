using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using BrokerageApi.V1.Gateways.Interfaces;
using BrokerageApi.V1.Infrastructure;

namespace BrokerageApi.V1.Gateways
{
    public class ServiceOverviewGateway : IServiceOverviewGateway
    {
        private readonly BrokerageContext _context;

        public ServiceOverviewGateway(BrokerageContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<ServiceOverview>> GetBySocialCareIdAsync(string socialCareId)
        {
            return await _context.ServiceOverviews
                .Where(so => so.SocialCareId == socialCareId)
                .OrderBy(so => so.Name)
                .ToListAsync();
        }

        public async Task<ServiceOverview> GetBySocialCareIdAndServiceIdAsync(string socialCareId, int serviceId)
        {
            return await _context.ServiceOverviews
                .Include(so => so.Elements.OrderBy(e => e.StartDate))
                    .ThenInclude(e => e.Referral)
                        .ThenInclude(r => r.Workflows)
                .Include(so => so.Elements.OrderBy(e => e.StartDate))
                    .ThenInclude(e => e.Service)
                .Include(so => so.Elements.OrderBy(e => e.StartDate))
                    .ThenInclude(e => e.Provider)
                .Include(so => so.Elements.OrderBy(e => e.StartDate))
                    .ThenInclude(e => e.Suspensions)
                        .ThenInclude(s => s.Referral)
                            .ThenInclude(r => r.Workflows)
                .Where(so => so.SocialCareId == socialCareId)
                .Where(so => so.Id == serviceId)
                .SingleOrDefaultAsync();
        }
    }
}
