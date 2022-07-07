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
    [Route("api/v1/service-users")]
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
        [Route("{socialCareId}/serviceOverview")]
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
            catch (ArgumentException e)
            {
                return Problem(
                    e.Message,
                    $"api/v1/service-users/{socialCareId}/serviceOverview",
                    StatusCodes.Status404NotFound, "Not Found"
                );
            }
        }

        [Authorize(Roles = "Broker")]
        [HttpGet]
        [Route("{socialCareId}/care-packages")]
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
            catch (ArgumentNullException e)
            {
                return Problem(
                    e.Message,
                    $"/api/v1/service-users/{socialCareId}",
                    StatusCodes.Status404NotFound, "Not Found"
                );
            }
        }


        [HttpGet]
        [ProducesResponseType(typeof(List<ServiceUserResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
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
