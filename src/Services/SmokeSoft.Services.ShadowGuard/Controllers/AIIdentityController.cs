using Microsoft.AspNetCore.Mvc;
using SmokeSoft.Services.ShadowGuard.Services;
using SmokeSoft.Shared.DTOs.ShadowGuard;

namespace SmokeSoft.Services.ShadowGuard.Controllers;

[Route("api/shadowguard/ai-identities")]
[ApiController]
public class AIIdentityController : BaseController
{
    private readonly IAIIdentityService _aiIdentityService;

    public AIIdentityController(IAIIdentityService aiIdentityService)
    {
        _aiIdentityService = aiIdentityService;
    }

    [HttpGet]
    public async Task<IActionResult> GetUserAIIdentities(
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 10,
        CancellationToken cancellationToken = default)
    {
        var userId = GetUserId();
        var result = await _aiIdentityService.GetUserAIIdentitiesAsync(userId, pageNumber, pageSize, cancellationToken);

        if (!result.IsSuccess)
        {
            return BadRequest(new { error = result.ErrorMessage, code = result.ErrorCode });
        }

        return Ok(result.Data);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetAIIdentityById(Guid id, CancellationToken cancellationToken)
    {
        var userId = GetUserId();
        var result = await _aiIdentityService.GetAIIdentityByIdAsync(id, userId, cancellationToken);

        if (!result.IsSuccess)
        {
            return NotFound(new { error = result.ErrorMessage, code = result.ErrorCode });
        }

        return Ok(result.Data);
    }

    [HttpPost]
    public async Task<IActionResult> CreateAIIdentity([FromBody] CreateAIIdentityRequest request, CancellationToken cancellationToken)
    {
        var userId = GetUserId();
        var result = await _aiIdentityService.CreateAIIdentityAsync(userId, request, cancellationToken);

        if (!result.IsSuccess)
        {
            return BadRequest(new { error = result.ErrorMessage, code = result.ErrorCode });
        }

        return CreatedAtAction(nameof(GetAIIdentityById), new { id = result.Data!.Id }, result.Data);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateAIIdentity(Guid id, [FromBody] UpdateAIIdentityRequest request, CancellationToken cancellationToken)
    {
        var userId = GetUserId();
        var result = await _aiIdentityService.UpdateAIIdentityAsync(id, userId, request, cancellationToken);

        if (!result.IsSuccess)
        {
            return BadRequest(new { error = result.ErrorMessage, code = result.ErrorCode });
        }

        return Ok(result.Data);
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteAIIdentity(Guid id, CancellationToken cancellationToken)
    {
        var userId = GetUserId();
        var result = await _aiIdentityService.DeleteAIIdentityAsync(id, userId, cancellationToken);

        if (!result.IsSuccess)
        {
            return BadRequest(new { error = result.ErrorMessage, code = result.ErrorCode });
        }

        return NoContent();
    }
}
