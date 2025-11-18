using MediatR;
using Microsoft.AspNetCore.Mvc;
using SportAcademy.Application.Commands.AuthCommands.Login;
using SportAcademy.Application.Commands.AuthCommands.Register;

namespace SportAcademy.Web.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly IMediator _mediator;
        public AuthController(IMediator mediator)
        {
            _mediator = mediator;
        }

        [HttpPost("sign-up")]
        public IActionResult SignUp(RegisterCommand command)
        {
            var result = _mediator.Send(command);
            return Ok(result);
        }

        [HttpPost("login")]
        public IActionResult Login(LoginCommand command)
        {
            var result = _mediator.Send(command);
            return Ok(result);
        }
    }
}
