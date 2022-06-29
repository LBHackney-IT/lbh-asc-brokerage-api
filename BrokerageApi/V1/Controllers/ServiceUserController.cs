using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BrokerageApi.V1.Controllers.Parameters;
using BrokerageApi.V1.Boundary.Response;
using BrokerageApi.V1.Factories;
using BrokerageApi.V1.UseCase.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace BrokerageApi.V1.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/v1/service-user/{socialCareId}")]
    [Produces("application/json")]
    [ApiVersion("1.0")]
    public class ServiceUserController : BaseController
    {
        private readonly IGetServiceOverviewUseCase _serviceOverviewUseCase;

        private readonly IGetServiceUserByRequestUseCase _serviceUserByRequestUseCase;

        private readonly IGetCarePackagesByServiceUserIdUseCase _getCarePackagesByServiceUserIdUseCase;
        public ServiceUserController(
            IGetServiceOverviewUseCase serviceOverviewUseCase,
            IGetCarePackagesByServiceUserIdUseCase getCarePackagesByServiceUserIdUseCase,
            IGetServiceUserByRequestUseCase serviceUserByRequestUseCase
        )
        {
            _serviceOverviewUseCase = serviceOverviewUseCase;
            _getCarePackagesByServiceUserIdUseCase = getCarePackagesByServiceUserIdUseCase;
            _serviceUserByRequestUseCase = serviceUserByRequestUseCase;
        }

        [Authorize]
        [HttpGet]
        [Route("serviceOverview")]
        [ProducesResponseType(typeof(List<ElementResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetServiceOverview([FromRoute] string socialCareId)
        {
            try
            {
                var result = await _serviceOverviewUseCase.ExecuteAsync(socialCareId);
                return Ok(result.Select(e => e.ToResponse()).ToList());
            }
            catch (ArgumentException)
            {
                return Problem(
                    "The requested service user was not found",
                    $"api/v1/service-user/{socialCareId}/serviceOverview",
                    StatusCodes.Status404NotFound, "Not Found"
                );
            }
        }

        [Authorize(Roles = "Broker")]
        [HttpGet]
        [Route("care-packages")]
        [ProducesResponseType(typeof(List<CarePackageResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]

        public async Task<IActionResult> GetServiceUserCarePackages([FromRoute] string socialCareId)
        {
            try
            {
                var carePackages = await _getCarePackagesByServiceUserIdUseCase.ExecuteAsync(socialCareId);
                return Ok(carePackages.Select(r => r.ToResponse()).ToList());
            }
            catch (ArgumentNullException)
            {
                return Problem(
                    "No care packages found for this service user",
                    $"/api/v1/service-user/{socialCareId}",
                    StatusCodes.Status404NotFound, "Not Found"
                );
            }
        }


        [HttpGet]
        [Route("service-users")]
        [ProducesResponseType(typeof(List<ServiceUserResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]

        public async Task<IActionResult> GetServiceUser([FromQuery] GetServiceUserRequest request)
        {
            try
            {
                var serviceUser = await _serviceUserByRequestUseCase.ExecuteAsync(request);
                return Ok(serviceUser.Select(r => r.ToResponse()).ToList());
            }
            catch (ArgumentException)
            {
                return Problem(
                    "Invalid request",
                    $"/api/v1/service-users/",
                    StatusCodes.Status400BadRequest, "Bad Request"
                );
            }
        }


    }
}
