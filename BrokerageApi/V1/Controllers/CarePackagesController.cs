using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using BrokerageApi.V1.Boundary.Request;
using BrokerageApi.V1.Boundary.Response;
using BrokerageApi.V1.Factories;
using BrokerageApi.V1.Infrastructure;
using BrokerageApi.V1.UseCase.Interfaces;

namespace BrokerageApi.V1.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/v1/referrals/{referralId}/care-package")]
    [Produces("application/json")]
    [ApiVersion("1.0")]
    public class CarePackagesController : BaseController
    {
        private readonly IGetCarePackageByIdUseCase _getCarePackageByIdUseCase;
        private readonly IStartCarePackageUseCase _startCarePackageUseCase;
        private readonly ICreateElementUseCase _createElementUseCase;

        public CarePackagesController(
          IGetCarePackageByIdUseCase getCarePackageByIdUseCase,
          IStartCarePackageUseCase startCarePackageUseCase,
          ICreateElementUseCase createElementUseCase
        )
        {
            _getCarePackageByIdUseCase = getCarePackageByIdUseCase;
            _startCarePackageUseCase = startCarePackageUseCase;
            _createElementUseCase = createElementUseCase;
        }

        [Authorize]
        [HttpGet]
        [ProducesResponseType(typeof(ReferralResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetCarePackage([FromRoute] int referralId)
        {
            try
            {
                var carePackage = await _getCarePackageByIdUseCase.ExecuteAsync(referralId);
                return Ok(carePackage.ToResponse());
            }
            catch (ArgumentNullException)
            {
                return Problem(
                    "The requested care package was not found",
                    $"/api/v1/referrals/{referralId}/care-package",
                    StatusCodes.Status404NotFound, "Not Found"
                );
            }
        }

        [Authorize(Roles = "Broker")]
        [HttpPost]
        [Route("start")]
        [ProducesResponseType(typeof(ReferralResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status422UnprocessableEntity)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> StartCarePackage([FromRoute] int referralId)
        {
            try
            {
                var referral = await _startCarePackageUseCase.ExecuteAsync(referralId);
                return Ok(referral.ToResponse());
            }
            catch (ArgumentNullException)
            {
                return Problem(
                    "The requested referral was not found",
                    $"/api/v1/referrals/{referralId}/care-package/start",
                    StatusCodes.Status404NotFound, "Not Found"
                );
            }
            catch (InvalidOperationException)
            {
                return Problem(
                    "The requested referral was in an invalid state to start editing",
                    $"/api/v1/referrals/{referralId}/care-package/start",
                    StatusCodes.Status422UnprocessableEntity, "Unprocessable Entity"
                );
            }
            catch (UnauthorizedAccessException)
            {
                return Problem(
                    "The requested referral is not assigned to the user",
                    $"/api/v1/referrals/{referralId}/care-package/start",
                    StatusCodes.Status403Forbidden, "Forbidden"
                );
            }
        }

        [Authorize(Roles = "Broker")]
        [HttpPost]
        [Route("elements")]
        [ProducesResponseType(typeof(ElementResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status422UnprocessableEntity)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> CreateElement([FromRoute] int referralId, [FromBody] CreateElementRequest request)
        {
            try
            {
                var element = await _createElementUseCase.ExecuteAsync(referralId, request);
                return Ok(element.ToResponse());
            }
            catch (ArgumentNullException)
            {
                return Problem(
                    "The requested referral was not found",
                    $"/api/v1/referrals/{referralId}/care-package/elements",
                    StatusCodes.Status404NotFound, "Not Found"
                );
            }
            catch (ArgumentException)
            {
                return Problem(
                    "The request was invalid",
                    $"/api/v1/referrals/{referralId}/care-package/elements",
                    StatusCodes.Status400BadRequest, "Bad Request"
                );
            }
            catch (InvalidOperationException)
            {
                return Problem(
                    "The requested referral was in an invalid state to add elements",
                    $"/api/v1/referrals/{referralId}/care-package/elements",
                    StatusCodes.Status400BadRequest, "Bad Request"
                );
            }
            catch (UnauthorizedAccessException)
            {
                return Problem(
                    "The requested referral is not assigned to the user",
                    $"/api/v1/referrals/{referralId}/care-package/elements",
                    StatusCodes.Status403Forbidden, "Forbidden"
                );
            }
        }
    }
}
