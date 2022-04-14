using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using BrokerageApi.V1.Gateways.Interfaces;
using BrokerageApi.V1.Infrastructure;

namespace BrokerageApi.V1.Gateways
{
    public class ServiceGateway : IServiceGateway
    {
        private readonly BrokerageContext _context;

        public ServiceGateway(BrokerageContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<Service>> GetAllAsync()
        {
            return await _context.Services
                .Where(s => s.IsArchived == false)
                .OrderByDescending(s => s.ParentId)
                .ThenBy(s => s.Position)
                .ToListAsync();
        }
    }
}
