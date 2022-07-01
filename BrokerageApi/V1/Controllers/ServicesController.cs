using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using BrokerageApi.V1.Boundary.Response;
using BrokerageApi.V1.Factories;
using BrokerageApi.V1.UseCase.Interfaces;

namespace BrokerageApi.V1.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/v1/services")]
    [Produces("application/json")]
    [ApiVersion("1.0")]
    public class ServicesController : BaseController
    {
        private readonly IGetAllServicesUseCase _getAllServicesUseCase;
        private readonly IGetServiceByIdUseCase _getServiceByIdUseCase;
        private readonly IFindProvidersByServiceIdUseCase _findProvidersByServiceIdUseCase;

        public ServicesController(
            IGetAllServicesUseCase getAllServicesUseCase,
            IGetServiceByIdUseCase getServiceByIdUseCase,
            IFindProvidersByServiceIdUseCase findProvidersByServiceIdUseCase
        )
        {
            _getAllServicesUseCase = getAllServicesUseCase;
            _getServiceByIdUseCase = getServiceByIdUseCase;
            _findProvidersByServiceIdUseCase = findProvidersByServiceIdUseCase;
        }

        [HttpGet]
        [ProducesResponseType(typeof(List<ServiceResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetAllServices()
        {
            var services = await _getAllServicesUseCase.ExecuteAsync();
            return Ok(services.Select(s => s.ToResponse()).ToList());
        }

        [HttpGet]
        [Route("{id}")]
        [ProducesResponseType(typeof(ServiceResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetService([FromRoute] int id)
        {
            try
            {
                var service = await _getServiceByIdUseCase.ExecuteAsync(id);
                return Ok(service.ToResponse());
            }
            catch (ArgumentNullException e)
            {
                return Problem(
                    e.Message,
                    $"/api/v1/services/{id}",
                    StatusCodes.Status404NotFound, "Not Found"
                );
            }
        }

        [HttpGet]
        [Route("{id}/providers")]
        [ProducesResponseType(typeof(List<ProviderResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> FindProvidersByService([FromRoute] int id, [FromQuery] string query)
        {
            try
            {
                var providers = await _findProvidersByServiceIdUseCase.ExecuteAsync(id, query);
                return Ok(providers.Select(s => s.ToResponse()).ToList());
            }
            catch (ArgumentNullException e)
            {
                return Problem(
                    e.Message,
                    $"/api/v1/services/{id}",
                    StatusCodes.Status404NotFound, "Not Found"
                );
            }
        }
    }
}
