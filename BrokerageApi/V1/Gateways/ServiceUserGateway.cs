using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using BrokerageApi.V1.Gateways.Interfaces;
using BrokerageApi.V1.Infrastructure;

namespace BrokerageApi.V1.Gateways
{
    public class ServiceUserGateway : IServiceUserGateway
    {
        private readonly BrokerageContext _context;

        public ServiceUserGateway(BrokerageContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<CarePackage>> GetByServiceUserIdAsync(string serviceUserId)
        {
            return await _context.CarePackages
                .Where(cp => cp.SocialCareId == serviceUserId)
                .Include(cp => cp.Elements.OrderBy(e => e.CreatedAt))
                .ThenInclude(e => e.Provider)
                .Include(cp => cp.Elements.OrderBy(e => e.CreatedAt))
                .ThenInclude(e => e.ElementType)
                .ThenInclude(et => et.Service)
                .ToListAsync();
        }
    }
}
