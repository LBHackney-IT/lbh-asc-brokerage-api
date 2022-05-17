using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using BrokerageApi.V1.Boundary.Response;
using BrokerageApi.V1.Factories;
using BrokerageApi.V1.Infrastructure;
using BrokerageApi.V1.UseCase.Interfaces;

namespace BrokerageApi.V1.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/v1/users")]
    [Produces("application/json")]
    [ApiVersion("1.0")]
    public class UsersController : BaseController
    {
        private readonly IGetAllUsersUseCase _getAllUsersUseCase;

        public UsersController(
          IGetAllUsersUseCase getAllUsersUseCase
        )
        {
            _getAllUsersUseCase = getAllUsersUseCase;
        }

        [HttpGet]
        [ProducesResponseType(typeof(List<UserResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetAllUsers([FromQuery] UserRole? role = null)
        {
            var users = await _getAllUsersUseCase.ExecuteAsync(role);
            return Ok(users.Select(u => u.ToResponse()).ToList());
        }
    }
}
