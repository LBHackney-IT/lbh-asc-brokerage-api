using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using BrokerageApi.V1.Gateways.Interfaces;
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
            var socialCareId = request.SocialCareId;
            var serviceUserName = request.ServiceUserName;
            var dateOfBirth = request.DateOfBirth;
            var isValidDate = (dateOfBirth.ToString() == "01 January 0001" ? false : true);

            if (socialCareId != null)
            {
                return await _context.ServiceUsers
                   .Where(u => u.SocialCareId == socialCareId)
                   .ToListAsync();
            }
            else if (isValidDate && serviceUserName != null)
            {
                return await _context.ServiceUsers
                    .Where(u => u.DateOfBirth == dateOfBirth)
                    .Where(u => u.ServiceUserName.Contains(serviceUserName))
                    .ToListAsync();
            }
            else if (isValidDate)
            {
                return await _context.ServiceUsers
                    .Where(u => u.DateOfBirth == dateOfBirth)
                    .ToListAsync();
            }
            else if (serviceUserName != null)
            {
                return await _context.ServiceUsers
                    .Where(u => u.ServiceUserName.Contains(serviceUserName))
                    .ToListAsync();
            }
            else
            {
                return null;
            }
        }

    }
}
