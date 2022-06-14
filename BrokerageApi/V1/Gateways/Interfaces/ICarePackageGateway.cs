using System.Collections.Generic;
using System.Threading.Tasks;
using BrokerageApi.V1.Infrastructure;

namespace BrokerageApi.V1.Gateways.Interfaces
{
    public interface ICarePackageGateway
    {
        public Task<CarePackage> GetByIdAsync(int id);

        public Task<IEnumerable<CarePackage>> GetByServiceUserIdAsync(string serviceUserId);

        public Task<IEnumerable<CarePackage>> GetByBudgetApprovalLimitAsync(decimal approvalLimit);

    }
}
