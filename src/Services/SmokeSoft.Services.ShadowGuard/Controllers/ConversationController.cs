using Microsoft.AspNetCore.Mvc;
using SmokeSoft.Services.ShadowGuard.Services;
using SmokeSoft.Shared.DTOs.ShadowGuard;

namespace SmokeSoft.Services.ShadowGuard.Controllers;

[Route("api/shadowguard/conversations")]
[ApiController]
public class ConversationController : BaseController
{
    private readonly IConversationService _conversationService;

    public ConversationController(IConversationService conversationService)
    {
        _conversationService = conversationService;
    }

    [HttpGet]
    public async Task<IActionResult> GetUserConversations(
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 20,
        CancellationToken cancellationToken = default)
    {
        var userId = GetUserId();
        var result = await _conversationService.GetUserConversationsAsync(userId, pageNumber, pageSize, cancellationToken);

        if (!result.IsSuccess)
        {
            return BadRequest(new { error = result.ErrorMessage, code = result.ErrorCode });
        }

        return Ok(result.Data);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetConversationById(Guid id, CancellationToken cancellationToken)
    {
        var userId = GetUserId();
        var result = await _conversationService.GetConversationByIdAsync(id, userId, cancellationToken);

        if (!result.IsSuccess)
        {
            return NotFound(new { error = result.ErrorMessage, code = result.ErrorCode });
        }

        return Ok(result.Data);
    }

    [HttpPost]
    public async Task<IActionResult> StartConversation([FromBody] StartConversationRequest request, CancellationToken cancellationToken)
    {
        var userId = GetUserId();
        var result = await _conversationService.StartConversationAsync(userId, request, cancellationToken);

        if (!result.IsSuccess)
        {
            return BadRequest(new { error = result.ErrorMessage, code = result.ErrorCode });
        }

        return CreatedAtAction(nameof(GetConversationById), new { id = result.Data!.Id }, result.Data);
    }

    [HttpPost("{id}/messages")]
    public async Task<IActionResult> SendMessage(Guid id, [FromBody] SendMessageRequest request, CancellationToken cancellationToken)
    {
        var userId = GetUserId();
        
        // Ensure conversation ID matches
        if (request.ConversationId != id)
        {
            return BadRequest(new { error = "Conversation ID mismatch" });
        }

        var result = await _conversationService.SendMessageAsync(userId, request, cancellationToken);

        if (!result.IsSuccess)
        {
            return BadRequest(new { error = result.ErrorMessage, code = result.ErrorCode });
        }

        return Ok(result.Data);
    }

    [HttpPost("{id}/end")]
    public async Task<IActionResult> EndConversation(Guid id, CancellationToken cancellationToken)
    {
        var userId = GetUserId();
        var result = await _conversationService.EndConversationAsync(id, userId, cancellationToken);

        if (!result.IsSuccess)
        {
            return BadRequest(new { error = result.ErrorMessage, code = result.ErrorCode });
        }

        return Ok(new { message = "Conversation ended successfully" });
    }
}
