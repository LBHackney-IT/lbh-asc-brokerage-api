using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using BrokerageApi.V1.Gateways.Interfaces;
using BrokerageApi.V1.Infrastructure;

namespace BrokerageApi.V1.Gateways
{
    public class ElementTypeGateway : IElementTypeGateway
    {
        private readonly BrokerageContext _context;

        public ElementTypeGateway(BrokerageContext context)
        {
            _context = context;
        }

        public async Task<ElementType> GetByIdAsync(int id)
        {
            return await _context.ElementTypes
                .SingleOrDefaultAsync(et => et.Id == id);
        }
    }
}
