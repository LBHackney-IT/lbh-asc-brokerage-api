using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using BrokerageApi.V1.Gateways.Interfaces;
using BrokerageApi.V1.Infrastructure;

namespace BrokerageApi.V1.Gateways
{
    public class CarePackageGateway : ICarePackageGateway
    {
        private readonly BrokerageContext _context;

        public CarePackageGateway(BrokerageContext context)
        {
            _context = context;
        }

        public async Task<CarePackage> GetByIdAsync(int id)
        {
            return await _context.CarePackages
                .Include(cp => cp.Elements.OrderBy(e => e.CreatedAt))
                    .ThenInclude(e => e.ParentElement)
                .Include(cp => cp.Elements.OrderBy(e => e.CreatedAt))
                    .ThenInclude(e => e.Provider)
                .Include(cp => cp.Elements.OrderBy(e => e.CreatedAt))
                    .ThenInclude(e => e.ElementType)
                        .ThenInclude(et => et.Service)
                .Include(cp => cp.Elements.OrderBy(e => e.CreatedAt))
                    .ThenInclude(e => e.SuspensionElements)
                .Include(cp => cp.AssignedBroker)
                .Include(cp => cp.AssignedApprover)
                .SingleOrDefaultAsync(cp => cp.Id == id);
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
                .Include(cp => cp.AssignedBroker)
                .Include(cp => cp.AssignedApprover)
                .ToListAsync();
        }

        public async Task<IEnumerable<CarePackage>> GetByBudgetApprovalLimitAsync(decimal approvalLimit)
        {
            return await _context.CarePackages
                .Where(x => x.Status == ReferralStatus.AwaitingApproval)
                .Where(c => c.EstimatedYearlyCost <= approvalLimit)
                .Include(cp => cp.Elements.OrderBy(e => e.CreatedAt))
                    .ThenInclude(e => e.Provider)
                .Include(cp => cp.Elements.OrderBy(e => e.CreatedAt))
                    .ThenInclude(e => e.ElementType)
                    .ThenInclude(et => et.Service)
                .Include(cp => cp.AssignedBroker)
                .Include(cp => cp.AssignedApprover)
                .ToListAsync();
        }

    }
}
