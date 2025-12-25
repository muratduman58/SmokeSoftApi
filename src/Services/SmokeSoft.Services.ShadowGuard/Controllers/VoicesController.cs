using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SmokeSoft.Services.ShadowGuard.Services;
using SmokeSoft.Shared.DTOs.ShadowGuard;

namespace SmokeSoft.Services.ShadowGuard.Controllers;

/// <summary>
/// ElevenLabs hazır ses kütüphanesi endpoint'leri
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class VoicesController : ControllerBase
{
    private readonly IElevenLabsVoiceService _elevenLabsService;
    private readonly ILogger<VoicesController> _logger;

    public VoicesController(
        IElevenLabsVoiceService elevenLabsService,
        ILogger<VoicesController> logger)
    {
        _elevenLabsService = elevenLabsService;
        _logger = logger;
    }

    /// <summary>
    /// ElevenLabs hazır ses kütüphanesini getirir (Oturum gerektirmez)
    /// </summary>
    /// <returns>Hazır sesler listesi</returns>
    [HttpGet("presets")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(List<PresetVoiceDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetPresetVoices(CancellationToken cancellationToken)
    {
        try
        {
            var voices = await _elevenLabsService.GetPresetVoicesAsync(cancellationToken);
            
            _logger.LogInformation("Retrieved {Count} preset voices for user", voices.Count);
            
            return Ok(voices);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving preset voices");
            return StatusCode(500, new { error = "Hazır sesler alınırken bir hata oluştu" });
        }
    }

    /// <summary>
    /// Belirli bir hazır sesin önizleme URL'sini getirir (Oturum gerektirir)
    /// </summary>
    /// <param name="voiceId">Ses ID'si</param>
    /// <returns>Önizleme URL'si</returns>
    [HttpGet("presets/{voiceId}/preview")]
    [Authorize]
    [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GetVoicePreview(string voiceId, CancellationToken cancellationToken)
    {
        try
        {
            var voices = await _elevenLabsService.GetPresetVoicesAsync(cancellationToken);
            var voice = voices.FirstOrDefault(v => v.VoiceId == voiceId);

            if (voice == null)
            {
                return NotFound(new { error = "Ses bulunamadı" });
            }

            return Ok(new 
            { 
                voiceId = voice.VoiceId,
                name = voice.Name,
                previewUrl = voice.PreviewUrl 
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving voice preview for {VoiceId}", voiceId);
            return StatusCode(500, new { error = "Ses önizlemesi alınırken bir hata oluştu" });
        }
    }
}
