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
    [Route("api/v1/services")]
    [Produces("application/json")]
    [ApiVersion("1.0")]
    public class ServicesController : BaseController
    {
        private readonly IGetAllServicesUseCase _getAllServicesUseCase;

        public ServicesController(
          IGetAllServicesUseCase getAllServicesUseCase
        )
        {
            _getAllServicesUseCase = getAllServicesUseCase;
        }

        [HttpGet]
        [ProducesResponseType(typeof(List<ServiceResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetAllServices()
        {
            var services = await _getAllServicesUseCase.ExecuteAsync();
            return Ok(services.Select(s => s.ToResponse()).ToList());
        }
    }
}
