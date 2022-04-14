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
    [Route("api/v1/referrals")]
    [Produces("application/json")]
    [ApiVersion("1.0")]
    public class ReferralsController : BaseController
    {
        private readonly ICreateReferralUseCase _createReferralUseCase;
        private readonly IGetAssignedReferralsUseCase _getAssignedReferralsUseCase;
        private readonly IGetCurrentReferralsUseCase _getCurrentReferralsUseCase;
        private readonly IGetReferralByIdUseCase _getReferralByIdUseCase;
        private readonly IAssignBrokerToReferralUseCase _assignBrokerToReferralUseCase;
        private readonly IReassignBrokerToReferralUseCase _reassignBrokerToReferralUseCase;

        public ReferralsController(
          ICreateReferralUseCase createReferralUseCase,
          IGetAssignedReferralsUseCase getAssignedReferralsUseCase,
          IGetCurrentReferralsUseCase getCurrentReferralsUseCase,
          IGetReferralByIdUseCase getReferralByIdUseCase,
          IAssignBrokerToReferralUseCase assignBrokerToReferralUseCase,
          IReassignBrokerToReferralUseCase reassignBrokerToReferralUseCase
        )
        {
            _createReferralUseCase = createReferralUseCase;
            _getAssignedReferralsUseCase = getAssignedReferralsUseCase;
            _getCurrentReferralsUseCase = getCurrentReferralsUseCase;
            _getReferralByIdUseCase = getReferralByIdUseCase;
            _assignBrokerToReferralUseCase = assignBrokerToReferralUseCase;
            _reassignBrokerToReferralUseCase = reassignBrokerToReferralUseCase;
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
            catch (ArgumentException)
            {
                return Problem(
                    "The request was invalid",
                    $"/api/v1/referrals",
                    StatusCodes.Status400BadRequest, "Bad Request"
                );
            }
            catch (InvalidOperationException)
            {
                return Problem(
                    "The workflow has already been referred",
                    $"/api/v1/referrals",
                    StatusCodes.Status422UnprocessableEntity, "Unprocessable Entity"
                );
            }
        }

        [HttpGet]
        [Route("assigned")]
        [ProducesResponseType(typeof(List<ReferralResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetAssignedReferrals([FromQuery] ReferralStatus? status = null)
        {
            var referrals = await _getAssignedReferralsUseCase.ExecuteAsync(User.Identity.Name, status);
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
            var referral = await _getReferralByIdUseCase.ExecuteAsync(id);

            if (referral is null)
            {
                return Problem(
                    "The requested referral was not found",
                    $"/api/v1/referrals/{id}",
                    StatusCodes.Status404NotFound, "Not Found"
                );
            }

            return Ok(referral.ToResponse());
        }

        [HttpPost]
        [Route("{id}/assign")]
        [ProducesResponseType(typeof(ReferralResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> AssignBroker([FromRoute] int id, [FromBody] AssignBrokerRequest request)
        {
            try
            {
                var referral = await _assignBrokerToReferralUseCase.ExecuteAsync(id, request);
                return Ok(referral.ToResponse());
            }
            catch (ArgumentException)
            {
                return Problem(
                    "The requested referral was not found",
                    $"/api/v1/referrals/{id}/assign",
                    StatusCodes.Status404NotFound, "Not Found"
                );
            }
            catch (InvalidOperationException)
            {
                return Problem(
                    "The requested referral was in an invalid state for assignment",
                    $"/api/v1/referrals/{id}/assign",
                    StatusCodes.Status400BadRequest, "Bad Request"
                );
            }
        }

        [HttpPost]
        [Route("{id}/reassign")]
        [ProducesResponseType(typeof(ReferralResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> ReassignBroker([FromRoute] int id, [FromBody] AssignBrokerRequest request)
        {
            try
            {
                var referral = await _reassignBrokerToReferralUseCase.ExecuteAsync(id, request);
                return Ok(referral.ToResponse());
            }
            catch (ArgumentException)
            {
                return Problem(
                    "The requested referral was not found",
                    $"/api/v1/referrals/{id}/reassign",
                    StatusCodes.Status404NotFound, "Not Found"
                );
            }
            catch (InvalidOperationException)
            {
                return Problem(
                    "The requested referral was in an invalid state for reassignment",
                    $"/api/v1/referrals/{id}/reassign",
                    StatusCodes.Status400BadRequest, "Bad Request"
                );
            }
        }
    }
}
