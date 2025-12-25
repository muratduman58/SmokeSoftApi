using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SmokeSoft.Services.ShadowGuard.Filters;
using SmokeSoft.Services.ShadowGuard.Services;
using SmokeSoft.Shared.DTOs.ShadowGuard;

namespace SmokeSoft.Services.ShadowGuard.Controllers;

/// <summary>
/// Ekran görüntüsü yükleme ve yönetimi endpoint'leri
/// </summary>
[Route("api/screenshots")]
[ApiController]
public class ScreenshotController : BaseController
{
    private readonly IScreenCustomizationService _screenCustomizationService;

    public ScreenshotController(IScreenCustomizationService screenCustomizationService)
    {
        _screenCustomizationService = screenCustomizationService;
    }

    /// <summary>
    /// Cihaz ekran görüntüsü yükler
    /// </summary>
    /// <param name="request">Yükleme bilgileri (deviceId, screenType)</param>
    /// <param name="file">Ekran görüntüsü dosyası (PNG, JPG, JPEG, WebP)</param>
    /// <remarks>
    /// Cihaz için özelleştirilmiş ekran görüntüsü yükler.
    /// 
    /// **Desteklenen Ekran Tipleri:**
    /// - `IncomingCall` - Gelen arama ekranı
    /// - `Conversation` - Konuşma ekranı
    /// 
    /// **Dosya Gereksinimleri:**
    /// - Maksimum boyut: 10MB
    /// - Desteklenen formatlar: PNG, JPG, JPEG, WebP
    /// - Önerilen boyutlar: 1080x1920 veya cihaz çözünürlüğü
    /// 
    /// **Dosya Kayıt Yeri:**
    /// - Dosyalar `uploads/screenshots/` klasörüne kaydedilir
    /// - Format: `{deviceId}_{screenType}_{guid}.{extension}`
    /// - Aynı cihaz ve ekran tipi için yeni yükleme yapılırsa eski dosya silinir
    /// 
    /// **Kullanım:**
    /// - Oturum açmış kullanıcılar için: Screenshot kullanıcıya bağlanır
    /// - Anonim kullanıcılar için: Screenshot sadece deviceId ile ilişkilendirilir
    /// 
    /// **Form Data Parametreleri:**
    /// - `deviceId` (string): Cihaz kimliği
    /// - `screenType` (string): Ekran tipi (IncomingCall, Conversation)
    /// - `file` (file): Yüklenecek görüntü dosyası
    /// </remarks>
    /// <response code="200">Screenshot başarıyla yüklendi</response>
    /// <response code="400">Geçersiz dosya veya parametreler</response>
    [HttpPost("upload")]
    [AllowAnonymous]
    [RequestSizeLimit(10 * 1024 * 1024)] // 10MB limit
    [Consumes("multipart/form-data")]
    [ProducesResponseType(typeof(ScreenCustomizationDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> UploadScreenshot(
        [FromForm] UploadScreenshotRequest request,
        IFormFile file,
        CancellationToken cancellationToken)
    {
        if (file == null || file.Length == 0)
        {
            return BadRequest(new { error = "No file uploaded" });
        }

        // Try to get user ID if authenticated
        Guid? userId = null;
        try
        {
            if (User.Identity?.IsAuthenticated == true)
            {
                userId = GetUserId();
            }
        }
        catch
        {
            // User not authenticated, continue without userId
        }

        using var stream = file.OpenReadStream();
        var result = await _screenCustomizationService.UploadScreenshotAsync(
            request,
            stream,
            file.FileName,
            file.ContentType,
            userId,
            cancellationToken
        );

        if (!result.IsSuccess)
        {
            return BadRequest(new { error = result.ErrorMessage, code = result.ErrorCode });
        }

        return Ok(result.Data);
    }

    /// <summary>
    /// Cihazın tüm ekran görüntülerini getirir
    /// </summary>
    /// <param name="deviceId">Cihaz kimliği</param>
    /// <remarks>
    /// Belirtilen cihaz için yüklenmiş tüm ekran görüntülerini listeler.
    /// Her ekran tipi için en son yüklenen görüntü döner.
    /// </remarks>
    /// <response code="200">Ekran görüntüleri başarıyla getirildi</response>
    /// <response code="400">Geçersiz deviceId</response>
    [HttpGet("device/{deviceId}")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(List<ScreenCustomizationDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> GetDeviceScreenshots(string deviceId, CancellationToken cancellationToken)
    {
        var result = await _screenCustomizationService.GetDeviceScreenshotsAsync(deviceId, cancellationToken);

        if (!result.IsSuccess)
        {
            return BadRequest(new { error = result.ErrorMessage, code = result.ErrorCode });
        }

        return Ok(result.Data);
    }

    /// <summary>
    /// Belirli bir ekran tipi için görüntü getirir
    /// </summary>
    /// <param name="deviceId">Cihaz kimliği</param>
    /// <param name="screenType">Ekran tipi (IncomingCall, Conversation)</param>
    /// <remarks>
    /// Cihaz için belirtilen ekran tipinin en son yüklenen görüntüsünü döner.
    /// Görüntü bulunamazsa 404 döner.
    /// </remarks>
    /// <response code="200">Ekran görüntüsü bulundu</response>
    /// <response code="404">Ekran görüntüsü bulunamadı</response>
    [HttpGet("device/{deviceId}/{screenType}")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(ScreenCustomizationDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetScreenshotByType(
        string deviceId,
        string screenType,
        CancellationToken cancellationToken)
    {
        var result = await _screenCustomizationService.GetScreenshotByTypeAsync(deviceId, screenType, cancellationToken);

        if (!result.IsSuccess)
        {
            return NotFound(new { error = result.ErrorMessage, code = result.ErrorCode });
        }

        return Ok(result.Data);
    }

    /// <summary>
    /// Ekran görüntüsü dosyasını indirir
    /// </summary>
    /// <param name="id">Screenshot ID'si</param>
    /// <remarks>
    /// Screenshot dosyasını binary olarak döner.
    /// Doğrudan tarayıcıda görüntülenebilir veya indirilebilir.
    /// </remarks>
    /// <response code="200">Dosya başarıyla döndürüldü</response>
    /// <response code="404">Screenshot bulunamadı</response>
    [HttpGet("{id}")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(FileResult), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetScreenshotFile(Guid id, CancellationToken cancellationToken)
    {
        var service = _screenCustomizationService as ScreenCustomizationService;
        if (service == null)
        {
            return StatusCode(500, new { error = "Service not available" });
        }

        var result = await service.GetScreenshotFileAsync(id, cancellationToken);

        if (!result.IsSuccess)
        {
            return NotFound(new { error = result.ErrorMessage });
        }

        var (fileStream, contentType, fileName) = result.Data!;
        return File(fileStream, contentType, fileName);
    }

    /// <summary>
    /// Ekran görüntüsünü siler
    /// </summary>
    /// <param name="id">Silinecek screenshot ID'si</param>
    /// <remarks>
    /// Kullanıcının yüklediği ekran görüntüsünü siler.
    /// Sadece screenshot'ı yükleyen kullanıcı silebilir.
    /// </remarks>
    /// <response code="200">Screenshot başarıyla silindi</response>
    /// <response code="400">Screenshot bulunamadı veya yetki yok</response>
    /// <response code="401">Oturum gerekli</response>
    [HttpDelete("{id}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> DeleteScreenshot(Guid id, CancellationToken cancellationToken)
    {
        var userId = GetUserId();
        var result = await _screenCustomizationService.DeleteScreenshotAsync(id, userId, cancellationToken);

        if (!result.IsSuccess)
        {
            return BadRequest(new { error = result.ErrorMessage, code = result.ErrorCode });
        }

        return Ok(new { message = "Screenshot deleted successfully" });
    }
}
