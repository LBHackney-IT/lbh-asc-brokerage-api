using System.Collections.Generic;
using System.Threading.Tasks;
using BrokerageApi.V1.Infrastructure;

namespace BrokerageApi.V1.Gateways.Interfaces
{
    public interface IReferralGateway
    {
        public Task<Referral> CreateAsync(Referral referral);
        public Task<IEnumerable<Referral>> GetCurrentAsync(ReferralStatus? status = null);
        public Task<Referral> GetByWorkflowIdAsync(string workflowId);
        public Task<Referral> GetByIdAsync(int id);
        public Task<Referral> GetByIdWithElementsAsync(int id);
        public Task<IEnumerable<Referral>> GetAssignedAsync(string email, ReferralStatus? status = null);
        public Task<IEnumerable<Referral>> GetApprovedAsync();
        public Task<IEnumerable<Referral>> GetFollowUpAsync();
        public Task<IEnumerable<Referral>> GetBySocialCareIdWithElementsAsync(string socialCareId);
    }
}
