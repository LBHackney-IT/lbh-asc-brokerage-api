using System.Security.Claims;

namespace BrokerageApi.V1.Services.Interfaces
{
    public interface IUserService
    {
        public ClaimsPrincipal Current { get; }
        public string Email { get; }
        public int UserId { get; }
    }
}
