using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using BrokerageApi.V1.Boundary.Request;
using BrokerageApi.V1.Boundary.Response;
using BrokerageApi.V1.Factories;
using BrokerageApi.V1.UseCase.Interfaces;
using BrokerageApi.V1.UseCase.Interfaces.CarePackageElements;

namespace BrokerageApi.V1.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/v1/referrals/{referralId}/care-package/elements")]
    [Produces("application/json")]
    [ApiVersion("1.0")]
    public class CarePackageElementsController : BaseController
    {
        private readonly ICreateElementUseCase _createElementUseCase;
        private readonly IDeleteElementUseCase _deleteElementUseCase;
        private readonly IEndElementUseCase _endElementUseCase;
        private readonly ICancelElementUseCase _cancelElementUseCase;
        private readonly ISuspendElementUseCase _suspendElementUseCase;
        private readonly IEditElementUseCase _editElementUseCase;

        public CarePackageElementsController(
            ICreateElementUseCase createElementUseCase,
            IDeleteElementUseCase deleteElementUseCase,
            IEndElementUseCase endElementUseCase,
            ICancelElementUseCase cancelElementUseCase,
            ISuspendElementUseCase suspendElementUseCase,
            IEditElementUseCase editElementUseCase)
        {
            _createElementUseCase = createElementUseCase;
            _deleteElementUseCase = deleteElementUseCase;
            _endElementUseCase = endElementUseCase;
            _cancelElementUseCase = cancelElementUseCase;
            _suspendElementUseCase = suspendElementUseCase;
            _editElementUseCase = editElementUseCase;
        }

        [Authorize(Roles = "Broker")]
        [HttpPost]
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
            catch (ArgumentNullException e)
            {
                return Problem(
                    e.Message,
                    $"/api/v1/referrals/{referralId}/care-package/elements",
                    StatusCodes.Status404NotFound, "Not Found"
                );
            }
            catch (ArgumentException e)
            {
                return Problem(
                    e.Message,
                    $"/api/v1/referrals/{referralId}/care-package/elements",
                    StatusCodes.Status400BadRequest, "Bad Request"
                );
            }
            catch (InvalidOperationException e)
            {
                return Problem(
                    e.Message,
                    $"/api/v1/referrals/{referralId}/care-package/elements",
                    StatusCodes.Status422UnprocessableEntity, "Unprocessable Entity"
                );
            }
            catch (UnauthorizedAccessException e)
            {
                return Problem(
                    e.Message,
                    $"/api/v1/referrals/{referralId}/care-package/elements",
                    StatusCodes.Status403Forbidden, "Forbidden"
                );
            }
        }

        [Authorize(Roles = "Broker")]
        [HttpDelete]
        [Route("{elementId}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status422UnprocessableEntity)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> DeleteElement([FromRoute] int referralId, [FromRoute] int elementId)
        {
            try
            {
                await _deleteElementUseCase.ExecuteAsync(referralId, elementId);
            }
            catch (ArgumentNullException)
            {
                return Problem(
                    "The requested referral was not found",
                    $"/api/v1/referrals/{referralId}/care-package/elements/{elementId}",
                    StatusCodes.Status404NotFound, "Not Found"
                );
            }
            catch (InvalidOperationException)
            {
                return Problem(
                    "The requested referral was in an invalid state to remove elements",
                    $"/api/v1/referrals/{referralId}/care-package/elements/{elementId}",
                    StatusCodes.Status422UnprocessableEntity, "Unprocessable Entity"
                );
            }
            catch (UnauthorizedAccessException)
            {
                return Problem(
                    "The requested referral is not assigned to the user",
                    $"/api/v1/referrals/{referralId}/care-package/elements/{elementId}",
                    StatusCodes.Status403Forbidden, "Forbidden"
                );
            }
            return Ok();
        }

        [HttpPost]
        [Route("{elementId}/end")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> EndElement([FromRoute] int referralId, [FromRoute] int elementId, [FromBody] EndRequest request)
        {
            try
            {
                await _endElementUseCase.ExecuteAsync(referralId, elementId, request.EndDate);
            }
            catch (ArgumentNullException e)
            {
                return Problem(
                    e.Message,
                    $"api/v1/referrals/{referralId}/care-package/elements/{elementId}/end",
                    StatusCodes.Status404NotFound, "Not Found"
                );
            }
            catch (InvalidOperationException e)
            {
                return Problem(
                    e.Message,
                    $"api/v1/referrals/{referralId}/care-package/elements/{elementId}/end",
                    StatusCodes.Status422UnprocessableEntity, "Unprocessable Entity"
                );
            }
            catch (ArgumentException e)
            {
                return Problem(
                    e.Message,
                    $"api/v1/referrals/{referralId}/care-package/elements/{elementId}/end",
                    StatusCodes.Status400BadRequest, "Bad Request"
                );
            }
            return Ok();
        }

        [HttpPost]
        [Route("{elementId}/cancel")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> CancelElement([FromRoute] int referralId, [FromRoute] int elementId)
        {
            try
            {
                await _cancelElementUseCase.ExecuteAsync(referralId, elementId);
            }
            catch (ArgumentNullException e)
            {
                return Problem(
                    e.Message,
                    $"api/v1/referrals/{referralId}/care-package/elements/{elementId}/end",
                    StatusCodes.Status404NotFound, "Not Found"
                );
            }
            catch (InvalidOperationException e)
            {
                return Problem(
                    e.Message,
                    $"api/v1/referrals/{referralId}/care-package/elements/{elementId}/end",
                    StatusCodes.Status422UnprocessableEntity, "Unprocessable Entity"
                );
            }
            return Ok();
        }

        [HttpPost]
        [Route("{elementId}/suspend")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> SuspendElement(int referralId, int elementId, SuspendRequest request)
        {
            try
            {
                await _suspendElementUseCase.ExecuteAsync(referralId, elementId, request.StartDate, request.EndDate);
                return Ok();
            }
            catch (ArgumentNullException e)
            {
                return Problem(
                    e.Message,
                    $"api/v1/referrals/{referralId}/care-package/elements/{elementId}/suspend",
                    StatusCodes.Status404NotFound, "Not Found"
                );
            }
            catch (InvalidOperationException e)
            {
                return Problem(
                    e.Message,
                    $"api/v1/referrals/{referralId}/care-package/elements/{elementId}/suspend",
                    StatusCodes.Status422UnprocessableEntity, "Unprocessable Entity"
                );
            }
            catch (ArgumentException e)
            {
                return Problem(
                    e.Message,
                    $"api/v1/referrals/{referralId}/care-package/elements/{elementId}/suspend",
                    StatusCodes.Status400BadRequest, "Bad Request"
                );
            }
        }

        [Authorize(Roles = "Broker")]
        [HttpPost]
        [Route("{elementId}/edit")]
        [ProducesResponseType(typeof(ElementResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status422UnprocessableEntity)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> EditElement([FromRoute] int referralId, [FromRoute] int elementId, [FromBody] EditElementRequest request)
        {
            try
            {
                var element = await _editElementUseCase.ExecuteAsync(referralId, elementId, request);
                return Ok(element.ToResponse());
            }
            catch (ArgumentNullException e)
            {
                return Problem(
                    e.Message,
                    $"/api/v1/referrals/{referralId}/care-package/elements/{elementId}/edit",
                    StatusCodes.Status404NotFound, "Not Found"
                );
            }
            catch (ArgumentException e)
            {
                return Problem(
                    e.Message,
                    $"/api/v1/referrals/{referralId}/care-package/elements/{elementId}/edit",
                    StatusCodes.Status400BadRequest, "Bad Request"
                );
            }
            catch (InvalidOperationException e)
            {
                return Problem(
                    e.Message,
                    $"/api/v1/referrals/{referralId}/care-package/elements/{elementId}/edit",
                    StatusCodes.Status422UnprocessableEntity, "Unprocessable Entity"
                );
            }
            catch (UnauthorizedAccessException e)
            {
                return Problem(
                    e.Message,
                    $"/api/v1/referrals/{referralId}/care-package/elements/{elementId}/edit",
                    StatusCodes.Status403Forbidden, "Forbidden"
                );
            }
        }
    }
}
