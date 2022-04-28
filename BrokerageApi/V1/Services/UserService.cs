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

        public ClaimsPrincipal Current
        {
            get => _context.HttpContext.User;
        }

        public string Name
        {
            get => Current.Identity.Name;
        }
    }
}
