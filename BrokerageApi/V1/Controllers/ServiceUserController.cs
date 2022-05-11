using System;
using System.Threading.Tasks;
using BrokerageApi.V1.Boundary.Response;
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
        public ServiceUserController(
            IGetServiceOverviewUseCase serviceOverviewUseCase
            )
        {
            _serviceOverviewUseCase = serviceOverviewUseCase;
        }

        [Authorize(Roles = "Broker")]
        [HttpGet]
        [ProducesResponseType(typeof(ServiceOverviewResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
        [Route("serviceOverview")]
        public Task<IActionResult> GetServiceOverview([FromRoute] int socialCareId)
        {
            throw new NotImplementedException();
        }
    }
}
