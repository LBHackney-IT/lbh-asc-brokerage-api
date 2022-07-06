using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using BrokerageApi.V1.Gateways.Interfaces;
using BrokerageApi.V1.Infrastructure;
using BrokerageApi.Tests.V1.Gateways.Helpers;
namespace BrokerageApi.V1.Gateways
{
    public class ProviderGateway : IProviderGateway
    {
        private readonly BrokerageContext _context;

        public ProviderGateway(BrokerageContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<Provider>> FindAsync(string query)
        {
            if (String.IsNullOrWhiteSpace(query))
            {
                return new List<Provider>();
            }
            else
            {
                return await _context.Providers
                    .Where(p => p.IsArchived == false)
                    .Where(p => p.SearchVector.Matches(EF.Functions.ToTsQuery("simple", ParsingHelpers.ParsedQuery(query))))
                    .OrderBy(p => p.Name)
                    .ToListAsync();
            }
        }

        public async Task<Provider> GetByIdAsync(int id)
        {
            return await _context.Providers
                .SingleOrDefaultAsync(p => p.Id == id);
        }

    }
}
