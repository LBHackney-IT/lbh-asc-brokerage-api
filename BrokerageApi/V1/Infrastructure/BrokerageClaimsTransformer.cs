using System;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;
using BrokerageApi.V1.Gateways.Interfaces;

namespace BrokerageApi.V1.Infrastructure
{
    public class BrokerageClaimsTransformer : IClaimsTransformation
    {
        private readonly IUserGateway _userGateway;

        public BrokerageClaimsTransformer(IUserGateway userGateway)
        {
            _userGateway = userGateway;
        }

        public async Task<ClaimsPrincipal> TransformAsync(ClaimsPrincipal principal)
        {
            var identity = (ClaimsIdentity) principal.Identity;

            if (principal.HasClaim("groups", "saml-socialcare-corepathwayspilot"))
            {
                var referrerClaim = new Claim(identity.RoleClaimType, "Referrer");
                identity.AddClaim(referrerClaim);
            }

            var email = identity.Name;
            var user = await _userGateway.GetByEmailAsync(email);

            if (user is null)
            {
                var name = principal.FindFirst(ClaimTypes.Name).Value;
                user = await _userGateway.CreateUser(email, name);
            }

            if (user.Roles != null)
            {
                foreach (var role in user.Roles)
                {
                    var claim = new Claim(identity.RoleClaimType, Enum.GetName(typeof(UserRole), role));
                    identity.AddClaim(claim);
                }
            }

            var idClaim = new Claim(ClaimTypes.PrimarySid, user.Id.ToString());
            identity.AddClaim(idClaim);

            return principal;
        }
    }
}
