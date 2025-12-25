using Microsoft.AspNetCore.Mvc;
using SmokeSoft.Services.ShadowGuard.Services;
using SmokeSoft.Shared.Common;
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

    /// <summary>
    /// Kullanıcının AI kimliklerini sayfalı olarak getirir
    /// </summary>
    /// <param name="pageNumber">Sayfa numarası (varsayılan: 1)</param>
    /// <param name="pageSize">Sayfa başına kayıt sayısı (varsayılan: 10, max: 50)</param>
    /// <remarks>
    /// Bu endpoint kullanıcının oluşturduğu tüm AI kimliklerini listeler.
    /// Her AI kimliği için ad, açıklama, ses bilgileri ve oluşturulma tarihi döner.
    /// </remarks>
    /// <response code="200">AI kimlikleri başarıyla getirildi</response>
    /// <response code="400">Geçersiz sayfa parametreleri</response>
    /// <response code="401">Oturum gerekli</response>
    [HttpGet]
    [ProducesResponseType(typeof(PagedResult<AIIdentityDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
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

    /// <summary>
    /// Belirli bir AI kimliğinin detaylarını getirir
    /// </summary>
    /// <param name="id">AI kimlik ID'si</param>
    /// <remarks>
    /// Sadece kullanıcının kendi AI kimliklerine erişebilir.
    /// AI kimliği bulunamazsa veya başka bir kullanıcıya aitse 404 döner.
    /// </remarks>
    /// <response code="200">AI kimliği başarıyla getirildi</response>
    /// <response code="404">AI kimliği bulunamadı</response>
    /// <response code="401">Oturum gerekli</response>
    [HttpGet("{id}")]
    [ProducesResponseType(typeof(AIIdentityDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
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

    /// <summary>
    /// Yeni bir AI kimliği oluşturur
    /// </summary>
    /// <param name="request">AI kimlik oluşturma bilgileri</param>
    /// <remarks>
    /// Yeni bir AI kimliği oluşturur. İsteğe bağlı olarak:
    /// - Hazır ses seçilebilir (presetVoiceId)
    /// - Ses örneği yüklenebilir (voiceSampleId)
    /// - Karakter özellikleri tanımlanabilir (personality, greeting, etc.)
    /// 
    /// Ses slotu otomatik olarak oluşturulur ve LRU cache ile yönetilir.
    /// </remarks>
    /// <response code="201">AI kimliği başarıyla oluşturuldu</response>
    /// <response code="400">Geçersiz istek veya validasyon hatası</response>
    /// <response code="401">Oturum gerekli</response>
    [HttpPost]
    [ProducesResponseType(typeof(AIIdentityDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
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

    /// <summary>
    /// Mevcut bir AI kimliğini günceller
    /// </summary>
    /// <param name="id">Güncellenecek AI kimlik ID'si</param>
    /// <param name="request">Güncelleme bilgileri</param>
    /// <remarks>
    /// AI kimliğinin adı, açıklaması, karakter özellikleri güncellenebilir.
    /// Sadece kullanıcının kendi AI kimliklerini güncelleyebilir.
    /// Ses değişikliği için yeni ses örneği yüklenebilir.
    /// </remarks>
    /// <response code="200">AI kimliği başarıyla güncellendi</response>
    /// <response code="400">Geçersiz istek</response>
    /// <response code="404">AI kimliği bulunamadı</response>
    /// <response code="401">Oturum gerekli</response>
    [HttpPut("{id}")]
    [ProducesResponseType(typeof(AIIdentityDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
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

    /// <summary>
    /// Bir AI kimliğini siler
    /// </summary>
    /// <param name="id">Silinecek AI kimlik ID'si</param>
    /// <remarks>
    /// AI kimliğini ve ilişkili tüm verileri (ses slotu, konuşmalar) siler.
    /// Sadece kullanıcının kendi AI kimliklerini silebilir.
    /// Silme işlemi geri alınamaz!
    /// </remarks>
    /// <response code="204">AI kimliği başarıyla silindi</response>
    /// <response code="400">Silme işlemi başarısız</response>
    /// <response code="404">AI kimliği bulunamadı</response>
    /// <response code="401">Oturum gerekli</response>
    [HttpDelete("{id}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
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
