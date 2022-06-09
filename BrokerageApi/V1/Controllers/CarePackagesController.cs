using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using BrokerageApi.V1.Boundary.Request;
using BrokerageApi.V1.Boundary.Response;
using BrokerageApi.V1.Factories;
using BrokerageApi.V1.UseCase.Interfaces;
using BrokerageApi.V1.UseCase.Interfaces.CarePackages;
using X.PagedList;

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
        private readonly IEndCarePackageUseCase _endCarePackageUseCase;
        private readonly ICancelCarePackageUseCase _cancelCarePackageUseCase;
        private readonly ISuspendCarePackageUseCase _suspendCarePackageUseCase;
        private readonly IGetBudgetApproversUseCase _getBudgetApproversUseCase;
        private readonly IAssignBudgetApproverToCarePackageUseCase _assignBudgetApproverToCarePackageUseCase;

        public CarePackagesController(IGetCarePackageByIdUseCase getCarePackageByIdUseCase,
            IStartCarePackageUseCase startCarePackageUseCase,
            IEndCarePackageUseCase endCarePackageUseCase,
            ICancelCarePackageUseCase cancelCarePackageUseCase,
            ISuspendCarePackageUseCase suspendCarePackageUseCase,
            IGetBudgetApproversUseCase getBudgetApproversUseCase,
            IAssignBudgetApproverToCarePackageUseCase assignBudgetApproverToCarePackageUseCase)
        {
            _getCarePackageByIdUseCase = getCarePackageByIdUseCase;
            _startCarePackageUseCase = startCarePackageUseCase;
            _endCarePackageUseCase = endCarePackageUseCase;
            _cancelCarePackageUseCase = cancelCarePackageUseCase;
            _suspendCarePackageUseCase = suspendCarePackageUseCase;
            _getBudgetApproversUseCase = getBudgetApproversUseCase;
            _assignBudgetApproverToCarePackageUseCase = assignBudgetApproverToCarePackageUseCase;
        }

        [Authorize]
        [HttpGet]
        [ProducesResponseType(typeof(CarePackageResponse), StatusCodes.Status200OK)]
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
        [Route("end")]
        [ProducesResponseType(typeof(ReferralResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status422UnprocessableEntity)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> EndCarePackage([FromRoute] int referralId, [FromBody] EndRequest request)
        {
            try
            {
                await _endCarePackageUseCase.ExecuteAsync(referralId, request.EndDate, request.Comment);
            }
            catch (ArgumentNullException e)
            {
                return Problem(
                    e.Message,
                    $"api/v1/referrals/{referralId}/care-package/end",
                    StatusCodes.Status404NotFound, "Not Found"
                );
            }
            catch (InvalidOperationException e)
            {
                return Problem(
                    e.Message,
                    $"api/v1/referrals/{referralId}/care-package/end",
                    StatusCodes.Status422UnprocessableEntity, "Unprocessable Entity"
                );
            }
            catch (ArgumentException e)
            {
                return Problem(
                    e.Message,
                    $"api/v1/referrals/{referralId}/care-package/end",
                    StatusCodes.Status400BadRequest, "Bad Request"
                );
            }
            return Ok();
        }

        [Authorize(Roles = "Broker")]
        [HttpPost]
        [Route("cancel")]
        [ProducesResponseType(typeof(ReferralResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status422UnprocessableEntity)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> CancelCarePackage([FromRoute] int referralId, CancelRequest cancelRequest)
        {
            try
            {
                await _cancelCarePackageUseCase.ExecuteAsync(referralId, cancelRequest.Comment);
            }
            catch (ArgumentNullException e)
            {
                return Problem(
                    e.Message,
                    $"api/v1/referrals/{referralId}/care-package/cancel",
                    StatusCodes.Status404NotFound, "Not Found"
                );
            }
            catch (InvalidOperationException e)
            {
                return Problem(
                    e.Message,
                    $"api/v1/referrals/{referralId}/care-package/cancel",
                    StatusCodes.Status422UnprocessableEntity, "Unprocessable Entity"
                );
            }
            catch (ArgumentException e)
            {
                return Problem(
                    e.Message,
                    $"api/v1/referrals/{referralId}/care-package/cancel",
                    StatusCodes.Status400BadRequest, "Bad Request"
                );
            }
            return Ok();
        }

        [Authorize(Roles = "Broker")]
        [HttpPost]
        [Route("suspend")]
        [ProducesResponseType(typeof(ReferralResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status422UnprocessableEntity)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> SuspendCarePackage([FromRoute] int referralId, [FromBody] SuspendRequest request)
        {
            try
            {
                await _suspendCarePackageUseCase.ExecuteAsync(referralId, request.StartDate, request.EndDate, request.Comment);
            }
            catch (ArgumentNullException e)
            {
                return Problem(
                    e.Message,
                    $"api/v1/referrals/{referralId}/care-package/suspend",
                    StatusCodes.Status404NotFound, "Not Found"
                );
            }
            catch (InvalidOperationException e)
            {
                return Problem(
                    e.Message,
                    $"api/v1/referrals/{referralId}/care-package/suspend",
                    StatusCodes.Status422UnprocessableEntity, "Unprocessable Entity"
                );
            }
            catch (ArgumentException e)
            {
                return Problem(
                    e.Message,
                    $"api/v1/referrals/{referralId}/care-package/suspend",
                    StatusCodes.Status400BadRequest, "Bad Request"
                );
            }
            return Ok();
        }

        [Authorize(Roles = "Broker")]
        [HttpGet]
        [Route("budget-approvers")]
        [ProducesResponseType(typeof(GetApproversResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetBudgetApprovers([FromRoute] int referralId)
        {
            try
            {
                var (approvers, estimatedYearlyCost) = await _getBudgetApproversUseCase.ExecuteAsync(referralId);

                var result = new GetApproversResponse
                {
                    Approvers = await approvers.Select(u => u.ToResponse()).ToListAsync(),
                    EstimatedYearlyCost = estimatedYearlyCost
                };

                return Ok(result);
            }
            catch (ArgumentNullException e)
            {
                return Problem(
                    e.Message,
                    $"api/v1/referrals/{referralId}/care-package/suspend",
                    StatusCodes.Status404NotFound, "Not Found"
                );
            }
            catch (InvalidOperationException e)
            {
                return Problem(
                    e.Message,
                    $"api/v1/referrals/{referralId}/care-package/suspend",
                    StatusCodes.Status422UnprocessableEntity, "Unprocessable Entity"
                );
            }
        }

        [Authorize(Roles = "Broker")]
        [HttpPost]
        [Route("assign-budget-approver")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status422UnprocessableEntity)]
        public async Task<IActionResult> AssignBudgetApprover([FromRoute] int referralId, AssignApproverRequest request)
        {
            try
            {
                await _assignBudgetApproverToCarePackageUseCase.ExecuteAsync(referralId, request.Approver);
                return Ok();
            }
            catch (ArgumentNullException e)
            {
                return Problem(
                    e.Message,
                    $"api/v1/referrals/{referralId}/care-package/assign-budget-approver",
                    StatusCodes.Status404NotFound, "Not Found"
                );
            }
            catch (InvalidOperationException e)
            {
                return Problem(
                    e.Message,
                    $"api/v1/referrals/{referralId}/care-package/assign-budget-approver",
                    StatusCodes.Status422UnprocessableEntity, "Unprocessable Entity"
                );
            }
            catch (UnauthorizedAccessException e)
            {
                return Problem(
                    e.Message,
                    $"api/v1/referrals/{referralId}/care-package/assign-budget-approver",
                    StatusCodes.Status403Forbidden, "Forbidden"
                );
            }
        }
    }
}
