using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using BrokerageApi.V1.Gateways.Interfaces;
using BrokerageApi.V1.Infrastructure;

namespace BrokerageApi.V1.Gateways
{
    public class ReferralGateway : IReferralGateway
    {
        private readonly BrokerageContext _context;

        public ReferralGateway(BrokerageContext context)
        {
            _context = context;
        }

        public async Task<Referral> CreateAsync(Referral referral)
        {
            var entry = _context.Referrals.Add(referral);
            await _context.SaveChangesAsync();

            return entry.Entity;
        }

        public async Task<Referral> GetByWorkflowIdAsync(string workflowId)
        {
            return await _context.Referrals
                .Where(r => r.WorkflowId == workflowId)
                .SingleOrDefaultAsync();
        }
    }
}
