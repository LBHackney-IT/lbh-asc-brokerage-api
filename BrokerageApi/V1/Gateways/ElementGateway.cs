using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using BrokerageApi.V1.Gateways.Interfaces;
using BrokerageApi.V1.Infrastructure;
using BrokerageApi.V1.Services.Interfaces;

namespace BrokerageApi.V1.Gateways
{
    public class ElementGateway : IElementGateway
    {
        private readonly BrokerageContext _context;
        private readonly IOrderedQueryable<Element> _currentElements;
        private readonly IClockService _clock;

        public ElementGateway(BrokerageContext context)
        {
            _context = context;
            _clock = context.Clock;

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

        public async Task<IEnumerable<Element>> GetBySocialCareId(string socialCareId)
        {
            return await _currentElements
                .Where(e => e.SocialCareId == socialCareId)
                .ToListAsync();
        }

        public async Task<IEnumerable<Element>> GetCurrentBySocialCareId(string socialCareId)
        {
            return await _currentElements
                .Where(e => e.SocialCareId == socialCareId)
                .Where(e => e.InternalStatus == ElementStatus.Approved)
                .Where(e => e.EndDate >= _clock.Today || e.EndDate == null)
                .ToListAsync();
        }

        public async Task<Element> GetByIdAsync(int id)
        {
            return await _currentElements
                .Where(e => e.Id == id)
                .Include(e => e.ReferralElements)
                .SingleOrDefaultAsync();
        }

        public async Task<IEnumerable<Element>> GetByProviderIdAsync(int? id)
        {
            return await _currentElements
                .Where(e => e.ProviderId == id)
                .ToListAsync();
        }

        public async Task AddElementAsync(Element element)
        {
            await _context.Elements.AddAsync(element);
            await _context.SaveChangesAsync();
        }
    }
}
