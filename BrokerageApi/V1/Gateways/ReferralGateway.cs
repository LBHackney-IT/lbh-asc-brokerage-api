using System.Collections.Generic;
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
        private readonly IOrderedQueryable<Referral> _currentReferrals;
        private readonly IOrderedQueryable<Referral> _careChargeReferrals;

        public ReferralGateway(BrokerageContext context)
        {
            _context = context;

            var baseReferrals = _context.Referrals
                .Include(r => r.AssignedBroker)
                .Include(r => r.AssignedApprover)
                .Include(r => r.ReferralAmendments)
                .Include(r => r.ReferralFollowUps)
                .ThenInclude(f => f.RequestedBy)
                .Include(r => r.Workflows);

            _currentReferrals = baseReferrals
                .Where(r => r.Status != ReferralStatus.Archived)
                .Where(r => r.Status != ReferralStatus.Approved)
                .OrderBy(r => r.Id);

            _careChargeReferrals = baseReferrals
                .Where(r => r.Status == ReferralStatus.Approved)
                .Where(r => r.CareChargesConfirmedAt == null)
                .OrderBy(r => r.Id);
        }

        public async Task<Referral> CreateAsync(Referral referral)
        {
            var entry = _context.Referrals.Add(referral);
            await _context.SaveChangesAsync();

            return entry.Entity;
        }

        public async Task<IEnumerable<Referral>> GetCurrentAsync(ReferralStatus? status = null)
        {
            if (status == null)
            {
                return await _currentReferrals
                    .ToListAsync();
            }
            else
            {
                return await _currentReferrals
                    .Where(r => r.Status == status)
                    .ToListAsync();
            }
        }

        public async Task<IEnumerable<Referral>> GetAssignedAsync(string email, ReferralStatus? status = null)
        {
            if (status == null)
            {
                return await _currentReferrals
                    .Where(r => r.AssignedBrokerEmail == email)
                    .ToListAsync();
            }
            else
            {
                return await _currentReferrals
                    .Where(r => r.AssignedBrokerEmail == email)
                    .Where(r => r.Status == status)
                    .ToListAsync();
            }
        }

        public async Task<IEnumerable<Referral>> GetApprovedAsync()
        {
            return await _careChargeReferrals
                .Where(r => r.ReferralFollowUps.Count == 0)
                .ToListAsync();
        }

        public async Task<IEnumerable<Referral>> GetFollowUpAsync()
        {
            return await _careChargeReferrals
                .Where(r => r.ReferralFollowUps.Count > 0)
                .ToListAsync();
        }

        public async Task<IEnumerable<Referral>> GetBySocialCareIdWithElementsAsync(string socialCareId)
        {
            var referrals = _context.Referrals
                .Include(r => r.AssignedBroker)
                .Include(r => r.AssignedApprover)
                .Include(r => r.ReferralAmendments)
                .Include(r => r.ReferralFollowUps)
                    .ThenInclude(f => f.RequestedBy)
                .Include(r => r.Workflows)
                .Where(r => r.SocialCareId == socialCareId);

            return await referrals.ToListAsync();
        }

        public async Task<Referral> GetByWorkflowIdAsync(string workflowId)
        {
            return await _context.Referrals
                .Include(r => r.AssignedBroker)
                .Include(r => r.AssignedApprover)
                .Include(r => r.ReferralAmendments)
                .Include(r => r.ReferralFollowUps)
                    .ThenInclude(f => f.RequestedBy)
                .Include(r => r.Workflows)
                .Where(r => r.WorkflowId == workflowId)
                .SingleOrDefaultAsync();
        }

        public async Task<Referral> GetByIdAsync(int id)
        {
            return await _context.Referrals
                .Where(r => r.Id == id)
                .Include(r => r.AssignedBroker)
                .Include(r => r.AssignedApprover)
                .Include(r => r.ReferralAmendments)
                .Include(r => r.ReferralFollowUps)
                    .ThenInclude(f => f.RequestedBy)
                .Include(r => r.ReferralElements)
                .Include(r => r.Workflows)
                .SingleOrDefaultAsync();
        }

        public async Task<Referral> GetByIdWithElementsAsync(int id)
        {
            return await _context.Referrals
                .Where(r => r.Id == id)
                .Include(r => r.AssignedBroker)
                .Include(r => r.AssignedApprover)
                .Include(r => r.Elements)
                    .ThenInclude(e => e.ParentElement)
                .Include(r => r.Elements)
                    .ThenInclude(e => e.ReferralElements)
                .Include(r => r.ReferralAmendments)
                .Include(r => r.ReferralFollowUps)
                    .ThenInclude(f => f.RequestedBy)
                .Include(r => r.Workflows)
                .SingleOrDefaultAsync();
        }
    }
}
