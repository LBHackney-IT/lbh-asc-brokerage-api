using System.Security.Claims;
using NodaTime;

namespace BrokerageApi.V1.Services.Interfaces
{
    public interface IUserService
    {
        public ClaimsPrincipal Current { get; }
        public string Name { get; }
        public int UserId { get; }
    }
}
