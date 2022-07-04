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
    [Route("api/v1/providers")]
    [Produces("application/json")]
    [ApiVersion("1.0")]
    public class ProvidersController : BaseController
    {
        private readonly IFindProvidersUseCase _findProvidersUseCase;

        public ProvidersController(IFindProvidersUseCase findProvidersUseCase)
        {
            _findProvidersUseCase = findProvidersUseCase;
        }

        [HttpGet]
        [ProducesResponseType(typeof(List<ProviderResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> FindProviders([FromQuery] string query)
        {
            var providers = await _findProvidersUseCase.ExecuteAsync(query);
            return Ok(providers.Select(s => s.ToResponse()).ToList());
        }
    }
}
