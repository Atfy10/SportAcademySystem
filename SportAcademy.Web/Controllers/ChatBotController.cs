using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SportAcademy.Application.Commands.ChatCommands.AddMessage;
using SportAcademy.Application.Commands.ChatCommands.CreateConversation;
using SportAcademy.Application.Commands.ChatCommands.SendMessageToBot;
using SportAcademy.Application.Queries.ChatQueries;


namespace SportAcademy.Web.Controllers;

[Authorize]
[ApiController]
[Route("api/[controller]")]
public class ChatController : ControllerBase
{
    private readonly IMediator _mediator;

    public ChatController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpPost("conversation")]
    public async Task<IActionResult> CreateConversation(CreateConversationCommand command)
    {
        var result = await _mediator.Send(command);
        return Ok(result);
    }

    [HttpPost("message")]
    public async Task<IActionResult> AddMessage(AddMessageCommand command)
    {
        var result = await _mediator.Send(command);
        return Ok(result);
    }

    [HttpPost("bot")]
    public async Task<IActionResult> SendToBot(SendMessageToBotCommand command)
    {
        var result = await _mediator.Send(command);
        return Ok(result);
    }

    [HttpGet("history/{conversationId:guid}")]
    public async Task<IActionResult> GetHistory(Guid conversationId, CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new GetConversationByIdQuery(conversationId), cancellationToken);
        return Ok(result);
    }
}
