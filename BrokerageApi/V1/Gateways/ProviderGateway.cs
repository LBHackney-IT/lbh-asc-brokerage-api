using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using BrokerageApi.V1.Gateways.Interfaces;
using BrokerageApi.V1.Infrastructure;

namespace BrokerageApi.V1.Gateways
{
    public class ProviderGateway : IProviderGateway
    {
        private readonly BrokerageContext _context;

        public ProviderGateway(BrokerageContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<Provider>> FindByServiceIdAsync(int serviceId, string query)
        {
            return await _context.Providers
                .Where(p => p.IsArchived == false)
                .Where(p => p.Services.Any(s => s.Id == serviceId))
                .Where(p => p.SearchVector.Matches(EF.Functions.ToTsQuery("simple", ParsedQuery(query))))
                .OrderBy(p => p.Name)
                .ToListAsync();
        }

        public async Task<Provider> GetByIdAsync(int id)
        {
            return await _context.Providers
                .SingleOrDefaultAsync(p => p.Id == id);
        }

        private static string ParsedQuery(string query)
        {
            var separators = new[] { " " };
            var options = StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries;

            var words = query.Split(separators, options).ToList();
            var terms = words.ConvertAll(w => $"{w}:*");

            return String.Join(" & ", terms);
        }
    }
}
