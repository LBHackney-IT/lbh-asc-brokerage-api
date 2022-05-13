using System.Linq;
using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using NodaTime;
using BrokerageApi.V1.Services.Interfaces;

namespace BrokerageApi.V1.Services
{
    public class UserService : IUserService
    {
        private readonly IHttpContextAccessor _context;

        public UserService(IHttpContextAccessor context)
        {
            _context = context;
        }

        public ClaimsPrincipal Current => _context.HttpContext.User;

        public string Name => Current.Identity.Name;

        public int UserId => int.Parse(Current.Claims.SingleOrDefault(c => c.Type == ClaimTypes.PrimarySid).Value);
    }
}
