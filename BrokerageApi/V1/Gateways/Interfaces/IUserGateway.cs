using System.Collections.Generic;
using System.Threading.Tasks;
using BrokerageApi.V1.Infrastructure;

namespace BrokerageApi.V1.Gateways.Interfaces
{
    public interface IUserGateway
    {
        public Task<IEnumerable<User>> GetAllAsync(UserRole? role = null);
        public Task<IEnumerable<User>> GetBudgetApproversAsync(decimal limit);
        public Task<User> GetByEmailAsync(string email);
        public Task<User> CreateUser(string email, string name);
    }
}
