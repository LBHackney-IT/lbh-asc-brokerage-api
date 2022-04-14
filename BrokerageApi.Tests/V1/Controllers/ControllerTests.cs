using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using NUnit.Framework;
using System.Security.Claims;
using BrokerageApi.V1.Controllers;

namespace BrokerageApi.Tests.V1.Controllers
{
    [TestFixture]
    public class ControllerTests
    {
        protected static T GetResultData<T>(IActionResult result)
        {
            return (T) (result as ObjectResult)?.Value;
        }

        protected static int? GetStatusCode(IActionResult result)
        {
            return (result as IStatusCodeActionResult).StatusCode;
        }

        protected static void SetupAuthentication(BaseController controller)
        {
            if (TestContext.CurrentContext.Test.Properties.ContainsKey("AsUser"))
            {
                var role = TestContext.CurrentContext.Test.Properties.Get("AsUser");
                var email = role switch
                {
                    "Broker" => "a.broker@hackney.gov.uk",
                    _ => "a.user@hackney.gov.uk"
                };

                ClaimsIdentity identity = new ClaimsIdentity();

                identity.AddClaim(new Claim(ClaimTypes.Name, (string) email));
                identity.AddClaim(new Claim(ClaimTypes.Role, (string) role));

                ClaimsPrincipal principal = new ClaimsPrincipal(identity);

                controller.ControllerContext = new ControllerContext
                {
                    HttpContext = new DefaultHttpContext { User = principal }
                };
            }
        }
    }
}
