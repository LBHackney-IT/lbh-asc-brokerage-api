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
            if (principal.HasClaim("groups", "saml-socialcare-corepathwayspilot"))
            {
                var identity = (ClaimsIdentity) principal.Identity;
                var claim = new Claim(identity.RoleClaimType, "Referrer");

                identity.AddClaim(claim);
            }
            else
            {
                var email = principal.Identity.Name;
                var user = await _userGateway.GetByEmailAsync(email);
                var identity = (ClaimsIdentity) principal.Identity;

                foreach (var role in user.Roles)
                {
                    var claim = new Claim(identity.RoleClaimType, Enum.GetName(typeof(UserRole), role));
                    identity.AddClaim(claim);
                }

                var idClaim = new Claim(ClaimTypes.PrimarySid, user.Id.ToString());
                identity.AddClaim(idClaim);
            }

            return principal;
        }
    }
}
