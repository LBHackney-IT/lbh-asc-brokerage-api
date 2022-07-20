using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using BrokerageApi.V1.Gateways.Interfaces;
using BrokerageApi.Tests.V1.Gateways.Helpers;
using BrokerageApi.V1.Infrastructure;
using BrokerageApi.V1.Controllers.Parameters;



namespace BrokerageApi.V1.Gateways
{
    public class ServiceUserGateway : IServiceUserGateway
    {
        private readonly BrokerageContext _context;



        public ServiceUserGateway(BrokerageContext context)
        {
            _context = context;
        }



        public async Task<IEnumerable<ServiceUser>> GetByRequestAsync(GetServiceUserRequest request)
        {
            var requestSocialCareId = request.SocialCareId;
            var requestServiceUserName = request.ServiceUserName;
            var requestDateOfBirth = request.DateOfBirth;
            var requestProvider = request.ProviderId;

            if (requestSocialCareId != null)
            {
                return await _context.ServiceUsers
                   .Include(u => u.CarePackages)
                   .Where(u => u.SocialCareId == requestSocialCareId)
                   .ToListAsync();
            }
            else if (requestDateOfBirth != null && requestServiceUserName != null)
            {
                return await _context.ServiceUsers
                    .Include(u => u.CarePackages)
                    .Where(u => u.DateOfBirth == requestDateOfBirth)
                    .Where(p => p.NameSearchVector.Matches(EF.Functions.ToTsQuery("simple", ParsingHelpers.ParsedQuery(requestServiceUserName))))
                    .ToListAsync();
            }
            else if (requestDateOfBirth != null)
            {
                return await _context.ServiceUsers
                    .Include(u => u.CarePackages)
                    .Where(u => u.DateOfBirth == requestDateOfBirth)
                    .ToListAsync();
            }
            else if (requestServiceUserName != null)
            {
                return await _context.ServiceUsers
                    .Include(u => u.CarePackages)
                    .Where(p => p.NameSearchVector.Matches(EF.Functions.ToTsQuery("simple", ParsingHelpers.ParsedQuery(requestServiceUserName))))
                    .ToListAsync();
            }

            else
            {
                return null;
            }
        }

    }
}
