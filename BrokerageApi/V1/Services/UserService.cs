using System.Security.Claims;
using Microsoft.AspNetCore.Http;
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

        public string Email => Current.Identity.Name;

        public int UserId => int.Parse(Current.FindFirst(ClaimTypes.PrimarySid).Value);
    }
}
