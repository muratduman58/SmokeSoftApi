using Microsoft.AspNetCore.Mvc;
using SmokeSoft.Services.ShadowGuard.Services;
using SmokeSoft.Shared.Common;
using SmokeSoft.Shared.DTOs.ShadowGuard;
using SmokeSoft.Shared.Models;

namespace SmokeSoft.Services.ShadowGuard.Controllers;

/// <summary>
/// AI kimliği ile konuşma yönetimi endpoint'leri
/// </summary>
[Route("api/shadowguard/conversations")]
[ApiController]
public class ConversationController : BaseController
{
    private readonly IConversationService _conversationService;

    public ConversationController(IConversationService conversationService)
    {
        _conversationService = conversationService;
    }

    /// <summary>
    /// Kullanıcının konuşma geçmişini sayfalı olarak getirir
    /// </summary>
    /// <param name="pageNumber">Sayfa numarası (varsayılan: 1)</param>
    /// <param name="pageSize">Sayfa başına kayıt sayısı (varsayılan: 20)</param>
    /// <remarks>
    /// Kullanıcının AI kimlikleri ile yaptığı tüm konuşmaları listeler.
    /// Her konuşma için AI kimliği, başlangıç/bitiş zamanı, mesaj sayısı döner.
    /// </remarks>
    /// <response code="200">Konuşmalar başarıyla getirildi</response>
    /// <response code="400">Geçersiz sayfa parametreleri</response>
    /// <response code="401">Oturum gerekli</response>
    [HttpGet]
    [ProducesResponseType(typeof(PagedResult<ConversationDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GetUserConversations(
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 20,
        CancellationToken cancellationToken = default)
    {
        var userId = GetUserId();
        var result = await _conversationService.GetUserConversationsAsync(userId, pageNumber, pageSize, cancellationToken);

        if (!result.IsSuccess)
        {
            return Error(result.ErrorCode ?? "FETCH_FAILED", result.ErrorMessage ?? "Konuşmalar getirilemedi");
        }

        return Success(result.Data);
    }

    /// <summary>
    /// Belirli bir konuşmanın detaylarını ve mesajlarını getirir
    /// </summary>
    /// <param name="id">Konuşma ID'si</param>
    /// <remarks>
    /// Konuşma detayları ve tüm mesajları kronolojik sırada döner.
    /// Sadece kullanıcının kendi konuşmalarına erişebilir.
    /// </remarks>
    /// <response code="200">Konuşma detayları getirildi</response>
    /// <response code="404">Konuşma bulunamadı</response>
    /// <response code="401">Oturum gerekli</response>
    [HttpGet("{id}")]
    [ProducesResponseType(typeof(ConversationDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GetConversationById(Guid id, CancellationToken cancellationToken)
    {
        var userId = GetUserId();
        var result = await _conversationService.GetConversationByIdAsync(id, userId, cancellationToken);

        if (!result.IsSuccess)
        {
            return NotFoundError(result.ErrorMessage ?? "Konuşma bulunamadı");
        }

        return Success(result.Data);
    }

    /// <summary>
    /// Yeni bir konuşma başlatır
    /// </summary>
    /// <param name="request">Konuşma başlatma bilgileri (AI kimlik ID'si)</param>
    /// <remarks>
    /// Belirtilen AI kimliği ile yeni konuşma oturumu başlatır.
    /// Konuşma ID'si döner, bu ID ile WebSocket bağlantısı kurulabilir.
    /// WebSocket endpoint: ws://localhost:5076/ws/conversation/{conversationId}
    /// </remarks>
    /// <response code="201">Konuşma başarıyla başlatıldı</response>
    /// <response code="400">Geçersiz AI kimlik ID'si veya kota aşımı</response>
    /// <response code="401">Oturum gerekli</response>
    [HttpPost]
    [ProducesResponseType(typeof(ConversationDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> StartConversation([FromBody] StartConversationRequest request, CancellationToken cancellationToken)
    {
        var userId = GetUserId();
        var result = await _conversationService.StartConversationAsync(userId, request, cancellationToken);

        if (!result.IsSuccess)
        {
            return Error(result.ErrorCode ?? "START_FAILED", result.ErrorMessage ?? "Konuşma başlatılamadı");
        }

        return CreatedAtAction(
            nameof(GetConversationById), 
            new { id = result.Data!.Id }, 
            ApiResponse<ConversationDto>.SuccessResult(result.Data, "Konuşma başarıyla başlatıldı")
        );
    }

    /// <summary>
    /// Konuşmaya metin mesajı gönderir
    /// </summary>
    /// <param name="id">Konuşma ID'si</param>
    /// <param name="request">Mesaj içeriği</param>
    /// <remarks>
    /// Metin tabanlı mesaj gönderir ve AI yanıtını alır.
    /// Ses konuşmaları için WebSocket kullanılmalıdır.
    /// </remarks>
    /// <response code="200">Mesaj gönderildi ve yanıt alındı</response>
    /// <response code="400">Geçersiz istek veya konuşma ID uyuşmazlığı</response>
    /// <response code="401">Oturum gerekli</response>
    [HttpPost("{id}/messages")]
    [ProducesResponseType(typeof(MessageDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> SendMessage(Guid id, [FromBody] SendMessageRequest request, CancellationToken cancellationToken)
    {
        var userId = GetUserId();
        
        // Ensure conversation ID matches
        if (request.ConversationId != id)
        {
            return ValidationError("Konuşma ID uyuşmazlığı", "URL'deki ID ile request body'deki ID eşleşmiyor");
        }

        var result = await _conversationService.SendMessageAsync(userId, request, cancellationToken);

        if (!result.IsSuccess)
        {
            return Error(result.ErrorCode ?? "SEND_FAILED", result.ErrorMessage ?? "Mesaj gönderilemedi");
        }

        return Success(result.Data, "Mesaj başarıyla gönderildi");
    }

    /// <summary>
    /// Konuşmayı sonlandırır
    /// </summary>
    /// <param name="id">Sonlandırılacak konuşma ID'si</param>
    /// <remarks>
    /// Aktif konuşmayı sonlandırır ve istatistikleri kaydeder.
    /// Kullanılan dakika ve kredi bilgileri güncellenir.
    /// </remarks>
    /// <response code="200">Konuşma başarıyla sonlandırıldı</response>
    /// <response code="400">Konuşma bulunamadı veya zaten sonlandırılmış</response>
    /// <response code="401">Oturum gerekli</response>
    [HttpPost("{id}/end")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> EndConversation(Guid id, CancellationToken cancellationToken)
    {
        var userId = GetUserId();
        var result = await _conversationService.EndConversationAsync(id, userId, cancellationToken);

        if (!result.IsSuccess)
        {
            return Error(result.ErrorCode ?? "END_FAILED", result.ErrorMessage ?? "Konuşma sonlandırılamadı");
        }

        return Success(new { }, "Konuşma başarıyla sonlandırıldı");
    }
}
