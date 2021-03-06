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
        private readonly IConfirmCareChargesUseCase _confirmCareChargesUseCase;
        private readonly ICreateCareChargeUseCase _createCareChargeUseCase;
        private readonly IDeleteCareChargeUseCase _deleteCareChargeUseCase;
        private readonly IEndCareChargeUseCase _endCareChargeUseCase;
        private readonly ICancelCareChargeUseCase _cancelCareChargeUseCase;
        private readonly ISuspendCareChargeUseCase _suspendCareChargeUseCase;
        private readonly IEditCareChargeUseCase _editCareChargeUseCase;
        private readonly IResetCareChargeUseCase _resetCareChargeUseCase;

        public CarePackageCareChargesController(
            IConfirmCareChargesUseCase confirmCareChargesUseCase,
            ICreateCareChargeUseCase createCareChargeUseCase,
            IDeleteCareChargeUseCase deleteCareChargeUseCase,
            IEndCareChargeUseCase endCareChargeUseCase,
            ICancelCareChargeUseCase cancelCareChargeUseCase,
            ISuspendCareChargeUseCase suspendCareChargeUseCase,
            IEditCareChargeUseCase editCareChargeUseCase,
            IResetCareChargeUseCase resetCareChargeUseCase)
        {
            _confirmCareChargesUseCase = confirmCareChargesUseCase;
            _createCareChargeUseCase = createCareChargeUseCase;
            _deleteCareChargeUseCase = deleteCareChargeUseCase;
            _endCareChargeUseCase = endCareChargeUseCase;
            _cancelCareChargeUseCase = cancelCareChargeUseCase;
            _suspendCareChargeUseCase = suspendCareChargeUseCase;
            _editCareChargeUseCase = editCareChargeUseCase;
            _resetCareChargeUseCase = resetCareChargeUseCase;
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

        [Authorize(Roles = "CareChargesOfficer")]
        [HttpPost]
        [Route("confirm")]
        [ProducesResponseType(typeof(ElementResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status422UnprocessableEntity)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> ConfirmCareCharges([FromRoute] int referralId)
        {
            try
            {
                await _confirmCareChargesUseCase.ExecuteAsync(referralId);
            }
            catch (ArgumentNullException e)
            {
                return Problem(
                    e.Message,
                    $"/api/v1/referrals/{referralId}/care-package/care-charges/confirm",
                    StatusCodes.Status404NotFound, "Not Found"
                );
            }
            catch (InvalidOperationException e)
            {
                return Problem(
                    e.Message,
                    $"/api/v1/referrals/{referralId}/care-package/care-charges/confirm",
                    StatusCodes.Status422UnprocessableEntity, "Unprocessable Entity"
                );
            }
            return Ok();
        }

        [Authorize(Roles = "CareChargesOfficer")]
        [HttpDelete]
        [Route("{elementId}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status422UnprocessableEntity)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> DeleteCareCharge([FromRoute] int referralId, [FromRoute] int elementId)
        {
            try
            {
                await _deleteCareChargeUseCase.ExecuteAsync(referralId, elementId);
            }
            catch (ArgumentNullException e)
            {
                return Problem(
                    e.Message,
                    $"/api/v1/referrals/{referralId}/care-package/care-charges/{elementId}",
                    StatusCodes.Status404NotFound, "Not Found"
                );
            }
            catch (InvalidOperationException e)
            {
                return Problem(
                    e.Message,
                    $"/api/v1/referrals/{referralId}/care-package/care-charges/{elementId}",
                    StatusCodes.Status422UnprocessableEntity, "Unprocessable Entity"
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
        public async Task<IActionResult> EndCareCharge([FromRoute] int referralId, [FromRoute] int elementId, [FromBody] EndRequest request)
        {
            try
            {
                await _endCareChargeUseCase.ExecuteAsync(referralId, elementId, request.EndDate);
            }
            catch (ArgumentNullException e)
            {
                return Problem(
                    e.Message,
                    $"api/v1/referrals/{referralId}/care-package/care-charges/{elementId}/end",
                    StatusCodes.Status404NotFound, "Not Found"
                );
            }
            catch (InvalidOperationException e)
            {
                return Problem(
                    e.Message,
                    $"api/v1/referrals/{referralId}/care-package/care-charges/{elementId}/end",
                    StatusCodes.Status422UnprocessableEntity, "Unprocessable Entity"
                );
            }
            catch (ArgumentException e)
            {
                return Problem(
                    e.Message,
                    $"api/v1/referrals/{referralId}/care-package/care-charges/{elementId}/end",
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
        public async Task<IActionResult> CancelCareCharge([FromRoute] int referralId, [FromRoute] int elementId, CancelRequest cancelRequest)
        {
            try
            {
                await _cancelCareChargeUseCase.ExecuteAsync(referralId, elementId, cancelRequest.Comment);
            }
            catch (ArgumentNullException e)
            {
                return Problem(
                    e.Message,
                    $"api/v1/referrals/{referralId}/care-package/care-charges/{elementId}/cancel",
                    StatusCodes.Status404NotFound, "Not Found"
                );
            }
            catch (InvalidOperationException e)
            {
                return Problem(
                    e.Message,
                    $"api/v1/referrals/{referralId}/care-package/care-charges/{elementId}/cancel",
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
        public async Task<IActionResult> SuspendCareCharge(int referralId, int elementId, SuspendRequest request)
        {
            try
            {
                await _suspendCareChargeUseCase.ExecuteAsync(referralId, elementId, request.StartDate, request.EndDate, request.Comment);
                return Ok();
            }
            catch (ArgumentNullException e)
            {
                return Problem(
                    e.Message,
                    $"api/v1/referrals/{referralId}/care-package/care-charges/{elementId}/suspend",
                    StatusCodes.Status404NotFound, "Not Found"
                );
            }
            catch (InvalidOperationException e)
            {
                return Problem(
                    e.Message,
                    $"api/v1/referrals/{referralId}/care-package/care-charges/{elementId}/suspend",
                    StatusCodes.Status422UnprocessableEntity, "Unprocessable Entity"
                );
            }
            catch (ArgumentException e)
            {
                return Problem(
                    e.Message,
                    $"api/v1/referrals/{referralId}/care-package/care-charges/{elementId}/suspend",
                    StatusCodes.Status400BadRequest, "Bad Request"
                );
            }
        }

        [Authorize(Roles = "CareChargesOfficer")]
        [HttpPost]
        [Route("{elementId}/edit")]
        [ProducesResponseType(typeof(ElementResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status422UnprocessableEntity)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> EditCareCharge([FromRoute] int referralId, [FromRoute] int elementId, [FromBody] EditCareChargeRequest request)
        {
            try
            {
                var element = await _editCareChargeUseCase.ExecuteAsync(referralId, elementId, request);
                return Ok(element.ToResponse());
            }
            catch (ArgumentNullException e)
            {
                return Problem(
                    e.Message,
                    $"/api/v1/referrals/{referralId}/care-package/care-charges/{elementId}/edit",
                    StatusCodes.Status404NotFound, "Not Found"
                );
            }
            catch (ArgumentException e)
            {
                return Problem(
                    e.Message,
                    $"/api/v1/referrals/{referralId}/care-package/care-charges/{elementId}/edit",
                    StatusCodes.Status400BadRequest, "Bad Request"
                );
            }
            catch (InvalidOperationException e)
            {
                return Problem(
                    e.Message,
                    $"/api/v1/referrals/{referralId}/care-package/care-charges/{elementId}/edit",
                    StatusCodes.Status422UnprocessableEntity, "Unprocessable Entity"
                );
            }
        }

        [HttpPost]
        [Route("{elementId}/reset")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status422UnprocessableEntity)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> ResetCareCharge([FromRoute] int referralId, [FromRoute] int elementId)
        {
            try
            {
                await _resetCareChargeUseCase.ExecuteAsync(referralId, elementId);
            }
            catch (ArgumentNullException e)
            {
                return Problem(
                    e.Message,
                    $"api/v1/referrals/{referralId}/care-package/care-charges/{elementId}/reset",
                    StatusCodes.Status404NotFound, "Not Found"
                );
            }
            catch (InvalidOperationException e)
            {
                return Problem(
                    e.Message,
                    $"api/v1/referrals/{referralId}/care-package/care-charges/{elementId}/reset",
                    StatusCodes.Status422UnprocessableEntity, "Unprocessable Entity"
                );
            }
            return Ok();
        }
    }
}
