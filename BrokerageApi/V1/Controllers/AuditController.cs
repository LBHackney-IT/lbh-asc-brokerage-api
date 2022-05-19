using System.ComponentModel.DataAnnotations;
using System.Linq;
using BrokerageApi.V1.Boundary.Response;
using BrokerageApi.V1.Factories;
using BrokerageApi.V1.UseCase.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using X.PagedList;

namespace BrokerageApi.V1.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/v1/serviceuser/{socialCareId}")]
    [Produces("application/json")]
    [ApiVersion("1.0")]
    public class AuditController : BaseController
    {
        private readonly IGetServiceUserAuditEventsUseCase _auditEventUseCase;

        public AuditController(IGetServiceUserAuditEventsUseCase auditEventUseCase)
        {
            _auditEventUseCase = auditEventUseCase;
        }

        [HttpGet]
        [ProducesResponseType(typeof(GetServiceUserAuditEventsResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
        public IActionResult GetAuditEvents(
            [FromRoute] string socialCareId,
            [FromQuery][BindRequired][Range(1, int.MaxValue)] int pageNumber,
            [FromQuery][BindRequired][Range(1, 250)] int pageSize)
        {
            var auditEvents = _auditEventUseCase.Execute(socialCareId, pageNumber, pageSize);

            var result = new GetServiceUserAuditEventsResponse
            {
                Events = auditEvents.Select(ae => ae.ToResponse()).ToList(),
                PageMetadata = auditEvents.GetMetaData().ToResponse()
            };

            return Ok(result);
        }
    }

}
