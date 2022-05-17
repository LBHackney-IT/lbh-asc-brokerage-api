using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using BrokerageApi.V1.Gateways.Interfaces;
using BrokerageApi.V1.Infrastructure;

namespace BrokerageApi.V1.Gateways
{
    public class ElementGateway : IElementGateway
    {
        private readonly BrokerageContext _context;
        private readonly IOrderedQueryable<Element> _currentElements;

        public ElementGateway(BrokerageContext context)
        {
            _context = context;

            _currentElements = _context.Elements
                .Include(e => e.ElementType)
                    .ThenInclude(et => et.Service)
                .Include(e => e.Provider)
                .Include(e => e.ParentElement)
                .OrderBy(e => e.Id);
        }

        public async Task<IEnumerable<Element>> GetCurrentAsync()
        {
            return await _currentElements.ToListAsync();
        }

        public async Task<Element> GetByIdAsync(int id)
        {
            return await _currentElements
                .Where(e => e.Id == id)
                .SingleOrDefaultAsync();
        }
    }
}
