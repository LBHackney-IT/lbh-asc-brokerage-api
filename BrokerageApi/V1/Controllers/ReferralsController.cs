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
using X.PagedList;

namespace BrokerageApi.V1.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/v1/referrals")]
    [Produces("application/json")]
    [ApiVersion("1.0")]
    public class ReferralsController : BaseController
    {
        private readonly ICreateReferralUseCase _createReferralUseCase;
        private readonly IGetApprovedReferralsUseCase _getApprovedReferralsUseCase;
        private readonly IGetFollowUpReferralsUseCase _getFollowUpReferralsUseCase;
        private readonly IGetAssignedReferralsUseCase _getAssignedReferralsUseCase;
        private readonly IGetCurrentReferralsUseCase _getCurrentReferralsUseCase;
        private readonly IGetReferralByIdUseCase _getReferralByIdUseCase;
        private readonly IAssignBrokerToReferralUseCase _assignBrokerToReferralUseCase;
        private readonly IReassignBrokerToReferralUseCase _reassignBrokerToReferralUseCase;
        private readonly IArchiveReferralUseCase _archiveReferralUseCase;
        private readonly IGetBudgetApprovalsUseCase _getBudgetApprovalsUseCase;

        public ReferralsController(ICreateReferralUseCase createReferralUseCase,
            IGetApprovedReferralsUseCase getApprovedReferralsUseCase,
            IGetFollowUpReferralsUseCase getFollowUpReferralsUseCase,
            IGetAssignedReferralsUseCase getAssignedReferralsUseCase,
            IGetCurrentReferralsUseCase getCurrentReferralsUseCase,
            IGetReferralByIdUseCase getReferralByIdUseCase,
            IAssignBrokerToReferralUseCase assignBrokerToReferralUseCase,
            IReassignBrokerToReferralUseCase reassignBrokerToReferralUseCase,
            IArchiveReferralUseCase archiveReferralUseCase,
            IGetBudgetApprovalsUseCase getBudgetApprovalsUseCase)
        {
            _createReferralUseCase = createReferralUseCase;
            _getApprovedReferralsUseCase = getApprovedReferralsUseCase;
            _getFollowUpReferralsUseCase = getFollowUpReferralsUseCase;
            _getAssignedReferralsUseCase = getAssignedReferralsUseCase;
            _getCurrentReferralsUseCase = getCurrentReferralsUseCase;
            _getReferralByIdUseCase = getReferralByIdUseCase;
            _assignBrokerToReferralUseCase = assignBrokerToReferralUseCase;
            _reassignBrokerToReferralUseCase = reassignBrokerToReferralUseCase;
            _archiveReferralUseCase = archiveReferralUseCase;
            _getBudgetApprovalsUseCase = getBudgetApprovalsUseCase;
        }

        [Authorize(Roles = "Referrer")]
        [HttpPost]
        [ProducesResponseType(typeof(ReferralResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status422UnprocessableEntity)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> CreateReferral([FromBody] CreateReferralRequest request)
        {
            try
            {
                var referral = await _createReferralUseCase.ExecuteAsync(request);
                return Ok(referral.ToResponse());
            }
            catch (ArgumentException e)
            {
                return Problem(
                    e.Message,
                    "/api/v1/referrals",
                    StatusCodes.Status400BadRequest, "Bad Request"
                );
            }
            catch (InvalidOperationException e)
            {
                return Problem(
                    e.Message,
                    "/api/v1/referrals",
                    StatusCodes.Status422UnprocessableEntity, "Unprocessable Entity"
                );
            }
        }

        [HttpGet]
        [Route("approved")]
        [ProducesResponseType(typeof(List<ReferralResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetApprovedReferrals()
        {
            var referrals = await _getApprovedReferralsUseCase.ExecuteAsync();
            return Ok(referrals.Select(r => r.ToResponse()).ToList());
        }

        [HttpGet]
        [Route("follow-up")]
        [ProducesResponseType(typeof(List<ReferralResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetFollowUpReferrals()
        {
            var referrals = await _getFollowUpReferralsUseCase.ExecuteAsync();
            return Ok(referrals.Select(r => r.ToResponse()).ToList());
        }

        [HttpGet]
        [Route("assigned")]
        [ProducesResponseType(typeof(List<ReferralResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetAssignedReferrals([FromQuery] ReferralStatus? status = null)
        {
            var referrals = await _getAssignedReferralsUseCase.ExecuteAsync(status);
            return Ok(referrals.Select(r => r.ToResponse()).ToList());
        }

        [HttpGet]
        [Route("current")]
        [ProducesResponseType(typeof(List<ReferralResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetCurrentReferrals([FromQuery] ReferralStatus? status = null)
        {
            var referrals = await _getCurrentReferralsUseCase.ExecuteAsync(status);
            return Ok(referrals.Select(r => r.ToResponse()).ToList());
        }

        [HttpGet]
        [Route("{id}")]
        [ProducesResponseType(typeof(ReferralResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetReferral([FromRoute] int id)
        {
            try
            {
                var referral = await _getReferralByIdUseCase.ExecuteAsync(id);
                return Ok(referral.ToResponse());
            }
            catch (ArgumentNullException e)
            {
                return Problem(
                    e.Message,
                    $"/api/v1/referrals/{id}",
                    StatusCodes.Status404NotFound, "Not Found"
                );
            }
        }

        [HttpPost]
        [Route("{id}/assign")]
        [ProducesResponseType(typeof(ReferralResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status422UnprocessableEntity)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> AssignBroker([FromRoute] int id, [FromBody] AssignBrokerRequest request)
        {
            try
            {
                var referral = await _assignBrokerToReferralUseCase.ExecuteAsync(id, request);
                return Ok(referral.ToResponse());
            }
            catch (ArgumentNullException e)
            {
                return Problem(
                    e.Message,
                    $"/api/v1/referrals/{id}/assign",
                    StatusCodes.Status404NotFound, "Not Found"
                );
            }
            catch (InvalidOperationException e)
            {
                return Problem(
                    e.Message,
                    $"/api/v1/referrals/{id}/assign",
                    StatusCodes.Status422UnprocessableEntity, "Unprocessable Entity"
                );
            }
        }

        [HttpPost]
        [Route("{id}/reassign")]
        [ProducesResponseType(typeof(ReferralResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status422UnprocessableEntity)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> ReassignBroker([FromRoute] int id, [FromBody] AssignBrokerRequest request)
        {
            try
            {
                var referral = await _reassignBrokerToReferralUseCase.ExecuteAsync(id, request);
                return Ok(referral.ToResponse());
            }
            catch (ArgumentNullException e)
            {
                return Problem(
                    e.Message,
                    $"/api/v1/referrals/{id}/reassign",
                    StatusCodes.Status404NotFound, "Not Found"
                );
            }
            catch (InvalidOperationException e)
            {
                return Problem(
                    e.Message,
                    $"/api/v1/referrals/{id}/reassign",
                    StatusCodes.Status422UnprocessableEntity, "Unprocessable Entity"
                );
            }
        }

        [HttpPost]
        [Route("{id}/archive")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status422UnprocessableEntity)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> ArchiveReferral([FromRoute] int id, [FromBody] ArchiveReferralRequest request)
        {
            try
            {
                await _archiveReferralUseCase.ExecuteAsync(id, request.Comment);
                return Ok();
            }
            catch (ArgumentNullException e)
            {
                return Problem(
                    e.Message,
                    $"/api/v1/referrals/{id}/archive",
                    StatusCodes.Status404NotFound, "Not Found"
                );
            }
            catch (InvalidOperationException e)
            {
                return Problem(
                    e.Message,
                    $"/api/v1/referrals/{id}/archive",
                    StatusCodes.Status422UnprocessableEntity, "Unprocessable Entity"
                );
            }
        }

        [Authorize(Roles = "Approver")]
        [HttpGet]
        [Route("budget-approvals")]
        [ProducesResponseType(typeof(List<CarePackageResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetBudgetApprovals()
        {
            try
            {
                var approvals = await (await _getBudgetApprovalsUseCase.ExecuteAsync())
                    .Select(a => a.ToResponse())
                    .ToListAsync();
                return Ok(approvals);
            }
            catch (UnauthorizedAccessException e)
            {
                return Problem(
                    e.Message,
                    $"/api/v1/referrals/budget-approvals",
                    StatusCodes.Status401Unauthorized, "Unauthorized"
                );
            }
        }
    }
}
