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
    [Route("api/v1/elements")]
    [Produces("application/json")]
    [ApiVersion("1.0")]
    public class ElementsController : BaseController
    {
        private readonly IGetCurrentElementsUseCase _getCurrentElementsUseCase;
        private readonly IGetElementByIdUseCase _getElementByIdUseCase;


        public ElementsController(
            IGetCurrentElementsUseCase getCurrentElementsUseCase,
                IGetElementByIdUseCase getElementByIdUseCase
        )
        {
            _getCurrentElementsUseCase = getCurrentElementsUseCase;
            _getElementByIdUseCase = getElementByIdUseCase;
        }

        [HttpGet]
        [Route("current")]
        [ProducesResponseType(typeof(List<ElementResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetCurrentElements()
        {
            var elements = await _getCurrentElementsUseCase.ExecuteAsync();
            return Ok(elements.Select(r => r.ToResponse()).ToList());
        }

        [HttpGet]
        [Route("{id}")]
        [ProducesResponseType(typeof(ElementResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetElement([FromRoute] int id)
        {
            try
            {
                var element = await _getElementByIdUseCase.ExecuteAsync(id);
                return Ok(element.ToResponse());
            }
            catch (ArgumentException)
            {
                return Problem(
                    "The requested element was not found",
                    $"/api/v1/elements/{id}",
                    StatusCodes.Status404NotFound, "Not Found"
                );
            }
        }
    }
}
