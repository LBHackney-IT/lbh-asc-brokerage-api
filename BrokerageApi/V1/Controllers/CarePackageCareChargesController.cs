using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using BrokerageApi.V1.Boundary.Request;
using BrokerageApi.V1.Boundary.Response;
using BrokerageApi.V1.Factories;
using BrokerageApi.V1.UseCase.Interfaces.CarePackageCareCharges;

namespace BrokerageApi.V1.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/v1/referrals/{referralId}/care-package/care-charges")]
    [Produces("application/json")]
    [ApiVersion("1.0")]
    public class CarePackageCareChargesController : BaseController
    {
        private readonly ICreateCareChargeUseCase _createCareChargeUseCase;

        public CarePackageCareChargesController(ICreateCareChargeUseCase createCareChargeUseCase)
        {
            _createCareChargeUseCase = createCareChargeUseCase;
        }

        [Authorize(Roles = "CareChargesOfficer")]
        [HttpPost]
        [ProducesResponseType(typeof(ElementResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status422UnprocessableEntity)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> CreateCareCharge([FromRoute] int referralId, [FromBody] CreateCareChargeRequest request)
        {
            try
            {
                var element = await _createCareChargeUseCase.ExecuteAsync(referralId, request);
                return Ok(element.ToResponse());
            }
            catch (ArgumentNullException e)
            {
                return Problem(
                    e.Message,
                    $"/api/v1/referrals/{referralId}/care-package/care-charges",
                    StatusCodes.Status404NotFound, "Not Found"
                );
            }
            catch (ArgumentException e)
            {
                return Problem(
                    e.Message,
                    $"/api/v1/referrals/{referralId}/care-package/care-charges",
                    StatusCodes.Status400BadRequest, "Bad Request"
                );
            }
            catch (InvalidOperationException e)
            {
                return Problem(
                    e.Message,
                    $"/api/v1/referrals/{referralId}/care-package/care-charges",
                    StatusCodes.Status422UnprocessableEntity, "Unprocessable Entity"
                );
            }
        }
    }
}
