using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SmokeSoft.Services.ShadowGuard.Services;
using SmokeSoft.Shared.DTOs.Auth;

namespace SmokeSoft.Services.ShadowGuard.Controllers;

/// <summary>
/// Kimlik doğrulama ve kullanıcı yönetimi endpoint'leri
/// </summary>
[Route("api/shadowguard/auth")]
[ApiController]
public class AuthController : BaseController
{
    private readonly IAuthService _authService;
    private readonly IOAuthService _oauthService;
    private readonly IDeviceService _deviceService;

    public AuthController(
        IAuthService authService,
        IOAuthService oauthService,
        IDeviceService deviceService)
    {
        _authService = authService;
        _oauthService = oauthService;
        _deviceService = deviceService;
    }

    /// <summary>
    /// Yeni kullanıcı kaydı oluşturur
    /// </summary>
    /// <param name="request">Kayıt bilgileri (email, şifre, ad)</param>
    /// <remarks>
    /// Email ve şifre ile yeni kullanıcı hesabı oluşturur.
    /// Başarılı kayıt sonrası otomatik olarak JWT token döner.
    /// </remarks>
    /// <response code="200">Kayıt başarılı, token döndürüldü</response>
    /// <response code="400">Geçersiz bilgiler veya email zaten kullanımda</response>
    [HttpPost("register")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(AuthResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Register([FromBody] RegisterRequest request, CancellationToken cancellationToken)
    {
        var result = await _authService.RegisterAsync(request, cancellationToken);

        if (!result.IsSuccess)
        {
            return Error(result.ErrorCode ?? "REGISTER_FAILED", result.ErrorMessage ?? "Kayıt başarısız");
        }

        return Success(result.Data, "Kullanıcı başarıyla kaydedildi");
    }

    /// <summary>
    /// Email ve şifre ile giriş yapar
    /// </summary>
    /// <param name="request">Giriş bilgileri (email, şifre)</param>
    /// <remarks>
    /// Başarılı giriş sonrası JWT access token ve refresh token döner.
    /// Access token 1 saat, refresh token 7 gün geçerlidir.
    /// </remarks>
    /// <response code="200">Giriş başarılı, token döndürüldü</response>
    /// <response code="400">Geçersiz email veya şifre</response>
    [HttpPost("login")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(AuthResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Login([FromBody] LoginRequest request, CancellationToken cancellationToken)
    {
        var result = await _authService.LoginAsync(request, cancellationToken);

        if (!result.IsSuccess)
        {
            return Error(result.ErrorCode ?? "LOGIN_FAILED", result.ErrorMessage ?? "Giriş başarısız");
        }

        return Success(result.Data, "Giriş başarılı");
    }

    /// <summary>
    /// OAuth ile giriş yapar (Google, Apple, Facebook)
    /// </summary>
    /// <param name="request">OAuth giriş bilgileri (provider, token)</param>
    /// <remarks>
    /// Desteklenen provider'lar: Google, Apple, Facebook.
    /// İlk girişte otomatik kullanıcı hesabı oluşturulur.
    /// </remarks>
    /// <response code="200">OAuth girişi başarılı</response>
    /// <response code="400">Geçersiz OAuth token</response>
    [HttpPost("oauth/login")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(AuthResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> OAuthLogin([FromBody] OAuthLoginRequest request, CancellationToken cancellationToken)
    {
        var result = await _oauthService.LoginWithOAuthAsync(request, cancellationToken);

        if (!result.IsSuccess)
        {
            return Error(result.ErrorCode ?? "OAUTH_LOGIN_FAILED", result.ErrorMessage ?? "OAuth girişi başarısız");
        }

        return Success(result.Data, "OAuth girişi başarılı");
    }

    /// <summary>
    /// Refresh token ile yeni access token alır
    /// </summary>
    /// <param name="request">Refresh token</param>
    /// <remarks>
    /// Access token süresi dolduğunda yeni token almak için kullanılır.
    /// Refresh token da yenilenir.
    /// </remarks>
    /// <response code="200">Yeni token başarıyla oluşturuldu</response>
    /// <response code="400">Geçersiz veya süresi dolmuş refresh token</response>
    [HttpPost("refresh")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(AuthResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> RefreshToken([FromBody] RefreshTokenRequest request, CancellationToken cancellationToken)
    {
        var result = await _authService.RefreshTokenAsync(request.RefreshToken, cancellationToken);

        if (!result.IsSuccess)
        {
            return Error(result.ErrorCode ?? "REFRESH_FAILED", result.ErrorMessage ?? "Token yenileme başarısız");
        }

        return Success(result.Data, "Token başarıyla yenilendi");
    }

    /// <summary>
    /// Kullanıcı oturumunu kapatır
    /// </summary>
    /// <remarks>
    /// Mevcut refresh token'ı geçersiz kılar.
    /// Tekrar giriş yapmak gerekir.
    /// </remarks>
    /// <response code="200">Çıkış başarılı</response>
    /// <response code="401">Oturum gerekli</response>
    [HttpPost("logout")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Logout(CancellationToken cancellationToken)
    {
        var userId = GetUserId();
        var result = await _authService.LogoutAsync(userId, cancellationToken);

        if (!result.IsSuccess)
        {
            return Error(result.ErrorCode ?? "LOGOUT_FAILED", result.ErrorMessage ?? "Çıkış başarısız");
        }

        return Success(new { }, "Çıkış başarılı");
    }

    /// <summary>
    /// Mevcut kullanıcının bilgilerini getirir
    /// </summary>
    /// <remarks>
    /// JWT token'dan kullanıcı bilgilerini döner.
    /// Profil bilgileri, email, kayıt tarihi vb.
    /// </remarks>
    /// <response code="200">Kullanıcı bilgileri döndürüldü</response>
    /// <response code="404">Kullanıcı bulunamadı</response>
    /// <response code="401">Oturum gerekli</response>
    [HttpGet("me")]
    [ProducesResponseType(typeof(UserDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GetCurrentUser(CancellationToken cancellationToken)
    {
        var userId = GetUserId();
        var result = await _authService.GetCurrentUserAsync(userId, cancellationToken);

        if (!result.IsSuccess)
        {
            return NotFoundError(result.ErrorMessage ?? "Kullanıcı bulunamadı");
        }

        return Success(result.Data);
    }

    /// <summary>
    /// Kullanıcı profil bilgilerini günceller
    /// </summary>
    /// <param name="request">Güncellenecek profil bilgileri</param>
    /// <remarks>
    /// Ad, soyad, profil fotoğrafı gibi bilgiler güncellenebilir.
    /// Email değiştirmek için ayrı endpoint kullanılmalı.
    /// </remarks>
    /// <response code="200">Profil başarıyla güncellendi</response>
    /// <response code="400">Geçersiz bilgiler</response>
    /// <response code="401">Oturum gerekli</response>
    [HttpPut("me")]
    [ProducesResponseType(typeof(UserDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> UpdateProfile([FromBody] UpdateProfileRequest request, CancellationToken cancellationToken)
    {
        var userId = GetUserId();
        var result = await _authService.UpdateProfileAsync(userId, request, cancellationToken);

        if (!result.IsSuccess)
        {
            return Error(result.ErrorCode ?? "UPDATE_FAILED", result.ErrorMessage ?? "Profil güncellenemedi");
        }

        return Success(result.Data, "Profil başarıyla güncellendi");
    }

    /// <summary>
    /// Kullanıcı şifresini değiştirir
    /// </summary>
    /// <param name="request">Mevcut ve yeni şifre</param>
    /// <remarks>
    /// Şifre değiştirmek için mevcut şifre doğrulanmalıdır.
    /// Yeni şifre en az 8 karakter olmalıdır.
    /// </remarks>
    /// <response code="200">Şifre başarıyla değiştirildi</response>
    /// <response code="400">Mevcut şifre yanlış veya yeni şifre geçersiz</response>
    /// <response code="401">Oturum gerekli</response>
    [HttpPost("change-password")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordRequest request, CancellationToken cancellationToken)
    {
        var userId = GetUserId();
        var result = await _authService.ChangePasswordAsync(userId, request, cancellationToken);

        if (!result.IsSuccess)
        {
            return Error(result.ErrorCode ?? "PASSWORD_CHANGE_FAILED", result.ErrorMessage ?? "Şifre değiştirilemedi");
        }

        return Success(new { }, "Şifre başarıyla değiştirildi");
    }

    /// <summary>
    /// Kullanıcının bağlı OAuth sağlayıcılarını listeler
    /// </summary>
    /// <remarks>
    /// Google, Apple, Facebook gibi bağlı hesapları gösterir.
    /// Her provider için bağlantı tarihi ve durum bilgisi döner.
    /// </remarks>
    /// <response code="200">OAuth sağlayıcıları listelendi</response>
    /// <response code="401">Oturum gerekli</response>
    [HttpGet("oauth/providers")]
    [ProducesResponseType(typeof(List<OAuthProviderDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GetOAuthProviders(CancellationToken cancellationToken)
    {
        var userId = GetUserId();
        var result = await _oauthService.GetUserOAuthProvidersAsync(userId, cancellationToken);

        if (!result.IsSuccess)
        {
            return Error(result.ErrorCode ?? "FETCH_FAILED", result.ErrorMessage ?? "OAuth sağlayıcıları getirilemedi");
        }

        return Success(result.Data);
    }

    /// <summary>
    /// OAuth sağlayıcı bağlantısını kaldırır
    /// </summary>
    /// <param name="provider">Kaldırılacak sağlayıcı (google, apple, facebook)</param>
    /// <remarks>
    /// Hesap bağlantısını kaldırır. 
    /// En az bir giriş yöntemi (email/şifre veya OAuth) kalmalıdır.
    /// </remarks>
    /// <response code="200">Bağlantı başarıyla kaldırıldı</response>
    /// <response code="400">Son giriş yöntemi kaldırılamaz</response>
    /// <response code="401">Oturum gerekli</response>
    [HttpDelete("oauth/providers/{provider}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> UnlinkOAuthProvider(string provider, CancellationToken cancellationToken)
    {
        var userId = GetUserId();
        var result = await _oauthService.UnlinkOAuthProviderAsync(userId, provider, cancellationToken);

        if (!result.IsSuccess)
        {
            return Error(result.ErrorCode ?? "UNLINK_FAILED", result.ErrorMessage ?? "Bağlantı kaldırılamadı");
        }

        return Success(new { }, $"{provider} bağlantısı başarıyla kaldırıldı");
    }

    /// <summary>
    /// Yeni cihaz kaydeder veya mevcut cihazı günceller
    /// </summary>
    /// <param name="request">Cihaz bilgileri (deviceId, platform, model, vb.)</param>
    /// <remarks>
    /// Cihaz kimlik doğrulama ve güvenlik için kaydedilir.
    /// Aynı deviceId ile tekrar istek yapılırsa güncelleme yapılır.
    /// Kullanıcı girişi yapmadan da cihaz kaydedilebilir (anonim).
    /// </remarks>
    /// <response code="200">Cihaz başarıyla kaydedildi</response>
    /// <response code="400">Geçersiz cihaz bilgileri</response>
    [HttpPost("device/register")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(DeviceDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> RegisterDevice([FromBody] DeviceInfoRequest request, CancellationToken cancellationToken)
    {
        // Device can be registered without user (anonymous)
        var result = await _deviceService.RegisterOrUpdateDeviceAsync(request, null, cancellationToken);

        if (!result.IsSuccess)
        {
            return Error(result.ErrorCode ?? "DEVICE_REGISTER_FAILED", result.ErrorMessage ?? "Cihaz kaydedilemedi");
        }

        return Success(result.Data, "Cihaz başarıyla kaydedildi");
    }

    /// <summary>
    /// Kullanıcının kayıtlı cihazlarını listeler
    /// </summary>
    /// <remarks>
    /// Kullanıcının tüm aktif cihazlarını gösterir.
    /// Her cihaz için platform, model, son kullanım tarihi bilgisi döner.
    /// </remarks>
    /// <response code="200">Cihazlar listelendi</response>
    /// <response code="401">Oturum gerekli</response>
    [HttpGet("devices")]
    [ProducesResponseType(typeof(List<DeviceDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GetUserDevices(CancellationToken cancellationToken)
    {
        var userId = GetUserId();
        var result = await _deviceService.GetUserDevicesAsync(userId, cancellationToken);

        if (!result.IsSuccess)
        {
            return Error(result.ErrorCode ?? "FETCH_FAILED", result.ErrorMessage ?? "Cihazlar getirilemedi");
        }

        return Success(result.Data);
    }

    /// <summary>
    /// Bir cihazı devre dışı bırakır
    /// </summary>
    /// <param name="deviceId">Devre dışı bırakılacak cihaz ID'si</param>
    /// <remarks>
    /// Cihazı devre dışı bırakır. 
    /// Devre dışı cihazdan giriş yapılamaz.
    /// Güvenlik amacıyla kayıp/çalıntı cihazlar için kullanılır.
    /// </remarks>
    /// <response code="200">Cihaz devre dışı bırakıldı</response>
    /// <response code="400">Cihaz bulunamadı veya başka kullanıcıya ait</response>
    /// <response code="401">Oturum gerekli</response>
    [HttpDelete("devices/{deviceId}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> DeactivateDevice(Guid deviceId, CancellationToken cancellationToken)
    {
        var userId = GetUserId();
        var result = await _deviceService.DeactivateDeviceAsync(deviceId, userId, cancellationToken);

        if (!result.IsSuccess)
        {
            return Error(result.ErrorCode ?? "DEACTIVATE_FAILED", result.ErrorMessage ?? "Cihaz devre dışı bırakılamadı");
        }

        return Success(new { }, "Cihaz başarıyla devre dışı bırakıldı");
    }
}
