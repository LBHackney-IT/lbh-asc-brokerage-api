using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BrokerageApi.V1.Controllers.Parameters;
using BrokerageApi.V1.Boundary.Request;
using BrokerageApi.V1.Boundary.Response;
using BrokerageApi.V1.Factories;
using BrokerageApi.V1.UseCase.Interfaces;
using BrokerageApi.V1.UseCase.Interfaces.ServiceUsers;
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
        private readonly IGetServiceOverviewsUseCase _getServiceOverviewsUseCase;
        private readonly IGetServiceOverviewByIdUseCase _getServiceOverviewByIdUseCase;
        private readonly IGetServiceUserByRequestUseCase _serviceUserByRequestUseCase;
        private readonly IGetCarePackagesByServiceUserIdUseCase _getCarePackagesByServiceUserIdUseCase;

        private readonly IEditServiceUserUseCase _editServiceUserUseCase;
        public ServiceUserController(
            IGetServiceOverviewsUseCase getServiceOverviewsUseCase,
            IGetServiceOverviewByIdUseCase getServiceOverviewByIdUseCase,
            IGetCarePackagesByServiceUserIdUseCase getCarePackagesByServiceUserIdUseCase,
            IGetServiceUserByRequestUseCase serviceUserByRequestUseCase,
            IEditServiceUserUseCase editServiceUserUseCase
        )
        {
            _getServiceOverviewsUseCase = getServiceOverviewsUseCase;
            _getServiceOverviewByIdUseCase = getServiceOverviewByIdUseCase;
            _getCarePackagesByServiceUserIdUseCase = getCarePackagesByServiceUserIdUseCase;
            _serviceUserByRequestUseCase = serviceUserByRequestUseCase;
            _editServiceUserUseCase = editServiceUserUseCase;
        }

        [Authorize]
        [HttpGet]
        [Route("{socialCareId}/services")]
        [ProducesResponseType(typeof(List<ServiceOverviewResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetServiceOverviews([FromRoute] string socialCareId)
        {
            try
            {
                var result = await _getServiceOverviewsUseCase.ExecuteAsync(socialCareId);
                return Ok(result.Select(e => e.ToResponse()).ToList());
            }
            catch (ArgumentNullException e)
            {
                return Problem(
                    e.Message,
                    $"api/v1/service-users/{socialCareId}/services",
                    StatusCodes.Status404NotFound, "Not Found"
                );
            }
        }

        [Authorize]
        [HttpGet]
        [Route("{socialCareId}/services/{serviceId}")]
        [ProducesResponseType(typeof(ServiceOverviewResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetServiceOverviewById([FromRoute] string socialCareId, [FromRoute] int serviceId)
        {
            try
            {
                var result = await _getServiceOverviewByIdUseCase.ExecuteAsync(socialCareId, serviceId);
                return Ok(result.ToResponse());
            }
            catch (ArgumentNullException e)
            {
                return Problem(
                    e.Message,
                    $"api/v1/service-users/{socialCareId}/services/{serviceId}",
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


        [Authorize(Roles = "CareChargesOfficer")]
        [HttpPost]
        [Route("cedar-number")]
        [ProducesResponseType(typeof(ElementResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status422UnprocessableEntity)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> UpdateServiceUserCedarNumber([FromBody] EditServiceUserRequest request)
        {
            try
            {
                var element = await _editServiceUserUseCase.ExecuteAsync(request);
                return Ok(element.ToResponse());
            }
            catch (ArgumentNullException e)
            {
                return Problem(
                    e.Message,
                    $"/api/v1/service-users/",
                    StatusCodes.Status404NotFound, "Not Found"
                );
            }
            catch (ArgumentException e)
            {
                return Problem(
                    e.Message,
                    $"/api/v1/service-users/",
                    StatusCodes.Status400BadRequest, "Bad Request"
                );
            }
            catch (InvalidOperationException e)
            {
                return Problem(
                    e.Message,
                    $"/api/v1/service-users/",
                    StatusCodes.Status422UnprocessableEntity, "Unprocessable Entity"
                );
            }
            catch (UnauthorizedAccessException e)
            {
                return Problem(
                    e.Message,
                    $"/api/v1/service-users/",
                    StatusCodes.Status403Forbidden, "Forbidden"
                );
            }
        }


    }
}
