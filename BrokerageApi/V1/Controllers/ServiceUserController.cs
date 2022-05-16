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
    [Route("api/v1/serviceusers/{ServiceUserId}/care-packages")]
    [Produces("application/json")]
    [ApiVersion("1.0")]
    public class ServiceUserController : BaseController
    {
        private readonly IGetCarePackagesByServiceUserIdUseCase _getCarePackagesByServiceUserIdUseCase;

        public ServiceUserController(
          IGetCarePackagesByServiceUserIdUseCase getCarePackagesByServiceUserIdUseCase

        )
        {
            _getCarePackagesByServiceUserIdUseCase = getCarePackagesByServiceUserIdUseCase;

        }

        [HttpGet]
        [ProducesResponseType(typeof(ReferralResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetServiceUserCarePackages([FromRoute] int ServiceUserId)
        {
            var carePackages = await _getCarePackagesByServiceUserIdUseCase.ExecuteAsync(ServiceUserId);
            return Ok(carePackages.Select(r => r.ToResponse()).ToList());
        }


    }

}
