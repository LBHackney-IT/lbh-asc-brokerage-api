using System;
using System.IdentityModel.Tokens.Jwt;
using System.Threading.Tasks;
using BrokerageApi.V1.Gateways.Interfaces;
using BrokerageApi.V1.UseCase.Interfaces;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Newtonsoft.Json.Linq;

namespace BrokerageApi.V1.Controllers
{
    public class UserRolesMiddleware
    {
        private readonly RequestDelegate _next;

        public UserRolesMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task InvokeAsync(HttpContext context,
            IUserGateway userGateway
        )
        {
            // Get email from JWT
            var handler = new JwtSecurityTokenHandler();
            string authHeader = context.Request.Headers[Constants.Authorization];

            // Remove `Bearer ` if present
            authHeader = authHeader.Replace("Bearer ", "");

            var jsonToken = handler.ReadToken(authHeader);
            var jsonString = jsonToken.ToString();

            // Until we can authenticate, split by '}.{'
            string[] tokens = jsonString.Split(new[] { "}.{" }, StringSplitOptions.None);
            var authObject = JObject.Parse("{" + tokens[1]);

            // Proper authentication will prevent the above - will be something like this
            // handler.ValidateToken(/* some service */)

            // Get roles from users table, set at x- header
            string email = authObject["email"]?.ToString();
            var user = await userGateway.GetByEmailAsync(email);
            if (user == null)
            {
                Console.WriteLine("No match for " + email);
                context.Request.Headers[Constants.UserRoles] = "";
            }
            else
            {
                context.Request.Headers[Constants.UserRoles] =
                    String.Join(",", user.Roles.ToArray());
            }

            // Hand over to next middleware
            if (_next != null)
                await _next(context).ConfigureAwait(false);
        }
    }

    public static class UserRolesMiddlewareExtensions
    {
        public static IApplicationBuilder UseUserRoles(
            this IApplicationBuilder builder)
        {
            return builder.UseMiddleware<UserRolesMiddleware>();
        }
    }
}
