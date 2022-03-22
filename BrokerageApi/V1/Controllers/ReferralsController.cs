using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using BrokerageApi.V1.Boundary.Request;
using BrokerageApi.V1.Boundary.Response;
using BrokerageApi.V1.Factories;
using BrokerageApi.V1.UseCase.Interfaces;

namespace BrokerageApi.V1.Controllers
{
    [ApiController]
    [Route("api/v1/referrals")]
    [Produces("application/json")]
    [ApiVersion("1.0")]
    public class ReferralsController : BaseController
    {
        private readonly ICreateReferralUseCase _createReferralUseCase;

        public ReferralsController(
          ICreateReferralUseCase createReferralUseCase
        )
        {
            _createReferralUseCase = createReferralUseCase;
        }

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
    }
}
