using System.Net.WebSockets;
using System.Text;
using System.Text.Json;
using SmokeSoft.Services.ShadowGuard.Services;

namespace SmokeSoft.Services.ShadowGuard.WebSockets;

public class ElevenLabsConversationHandler
{
    private readonly IConversationService _conversationService;
    private readonly IVoiceSlotManager _voiceSlotManager;
    private readonly IElevenLabsVoiceService _elevenLabsService;
    private readonly IQuotaEnforcementService _quotaService;
    private readonly ILogger<ElevenLabsConversationHandler> _logger;

    public ElevenLabsConversationHandler(
        IConversationService conversationService,
        IVoiceSlotManager voiceSlotManager,
        IElevenLabsVoiceService elevenLabsService,
        IQuotaEnforcementService quotaService,
        ILogger<ElevenLabsConversationHandler> logger)
    {
        _conversationService = conversationService;
        _voiceSlotManager = voiceSlotManager;
        _elevenLabsService = elevenLabsService;
        _quotaService = quotaService;
        _logger = logger;
    }

    public async Task HandleConversationAsync(
        HttpContext context,
        WebSocket webSocket,
        string conversationId)
    {
        string sessionId = Guid.NewGuid().ToString();
        ClientWebSocket? elevenLabsWs = null;

        try
        {
            // 1. Authenticate
            var userId = GetUserIdFromContext(context);
            if (userId == Guid.Empty)
            {
                await CloseWithError(webSocket, "Unauthorized");
                return;
            }

            // 2. Validate conversation ownership
            var conversation = await _conversationService.GetByIdAsync(
                Guid.Parse(conversationId),
                userId
            );

            if (conversation == null)
            {
                await CloseWithError(webSocket, "Conversation not found");
                return;
            }

            // 3. Pre-flight quota check
            var quotaCheck = await _quotaService.PreFlightCheckAsync(userId);
            if (!quotaCheck.IsSuccess)
            {
                await CloseWithError(webSocket, quotaCheck.ErrorMessage ?? "Quota limit exceeded");
                return;
            }

            // 4. Create session
            await _conversationService.CreateSessionAsync(
                conversation.Id,
                sessionId,
                context.Connection.Id
            );

            // 5. Ensure voice slot
            var voiceResult = await _voiceSlotManager.EnsureVoiceSlotAsync(conversation.AIIdentity);
            if (!voiceResult.IsSuccess)
            {
                await CloseWithError(webSocket, voiceResult.ErrorMessage ?? "Failed to get voice");
                return;
            }

            var voiceId = voiceResult.Data ?? throw new InvalidOperationException("Voice ID is null");

            // 6. Connect to ElevenLabs WebSocket
            elevenLabsWs = await _elevenLabsService.ConnectWebSocketAsync(voiceId);

            _logger.LogInformation("Started conversation {ConversationId} with voice {VoiceId}", conversationId, voiceId);

            // 7. Start bidirectional streaming
            var receiveTask = ReceiveFromClientAsync(
                webSocket,
                elevenLabsWs,
                conversation.Id,
                userId,
                sessionId
            );

            var sendTask = SendToClientAsync(
                elevenLabsWs,
                webSocket,
                conversation.Id,
                sessionId
            );

            await Task.WhenAny(receiveTask, sendTask);

            // 8. Cleanup
            await _conversationService.CloseConversationAsync(conversation.Id, sessionId, "COMPLETED");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "WebSocket error for conversation {ConversationId}", conversationId);
            await _conversationService.CloseConversationAsync(Guid.Parse(conversationId), sessionId, "ERROR");
            await CloseWithError(webSocket, "Internal error");
        }
        finally
        {
            elevenLabsWs?.Dispose();
        }
    }

    private async Task ReceiveFromClientAsync(
        WebSocket clientWs,
        ClientWebSocket elevenLabsWs,
        Guid conversationId,
        Guid userId,
        string sessionId)
    {
        var buffer = new byte[1024 * 16]; // 16KB chunks
        var startTime = DateTime.UtcNow;

        while (clientWs.State == WebSocketState.Open && elevenLabsWs.State == WebSocketState.Open)
        {
            try
            {
                var result = await clientWs.ReceiveAsync(
                    new ArraySegment<byte>(buffer),
                    CancellationToken.None
                );

                if (result.MessageType == WebSocketMessageType.Binary)
                {
                    // 1. Real-time quota check
                    var elapsed = (DateTime.UtcNow - startTime).TotalMinutes;
                    var quotaCheck = await _quotaService.CheckDuringConversationAsync(userId, elapsed);

                    if (!quotaCheck.IsSuccess)
                    {
                        await SendWarningAsync(clientWs, quotaCheck.ErrorMessage ?? "Limit exceeded");
                        await _conversationService.CloseConversationAsync(conversationId, sessionId, "LIMIT_EXCEEDED");
                        break;
                    }

                    // 2. Forward audio to ElevenLabs
                    var audioChunk = buffer.Take(result.Count).ToArray();
                    await elevenLabsWs.SendAsync(
                        new ArraySegment<byte>(audioChunk),
                        WebSocketMessageType.Binary,
                        result.EndOfMessage,
                        CancellationToken.None
                    );

                    // 3. Update session metrics
                    await _conversationService.UpdateSessionMetricsAsync(sessionId, audioChunk.Length, sent: true);
                }
                else if (result.MessageType == WebSocketMessageType.Close)
                {
                    await _conversationService.CloseConversationAsync(conversationId, sessionId, "USER_ENDED");
                    break;
                }
            }
            catch (WebSocketException ex)
            {
                _logger.LogError(ex, "Client WebSocket error");
                break;
            }
        }
    }

    private async Task SendToClientAsync(
        ClientWebSocket elevenLabsWs,
        WebSocket clientWs,
        Guid conversationId,
        string sessionId)
    {
        var buffer = new byte[1024 * 16];

        while (elevenLabsWs.State == WebSocketState.Open && clientWs.State == WebSocketState.Open)
        {
            try
            {
                var result = await elevenLabsWs.ReceiveAsync(
                    new ArraySegment<byte>(buffer),
                    CancellationToken.None
                );

                if (result.MessageType == WebSocketMessageType.Binary)
                {
                    // 1. Forward audio to client
                    var audioChunk = buffer.Take(result.Count).ToArray();
                    await clientWs.SendAsync(
                        new ArraySegment<byte>(audioChunk),
                        WebSocketMessageType.Binary,
                        result.EndOfMessage,
                        CancellationToken.None
                    );

                    // 2. Update session metrics
                    await _conversationService.UpdateSessionMetricsAsync(sessionId, audioChunk.Length, sent: false);
                }
                else if (result.MessageType == WebSocketMessageType.Close)
                {
                    break;
                }
            }
            catch (WebSocketException ex)
            {
                _logger.LogError(ex, "ElevenLabs WebSocket error");
                break;
            }
        }
    }

    private Guid GetUserIdFromContext(HttpContext context)
    {
        var userIdClaim = context.User.FindFirst("sub") ?? context.User.FindFirst("userId");
        if (userIdClaim != null && Guid.TryParse(userIdClaim.Value, out var userId))
        {
            return userId;
        }
        return Guid.Empty;
    }

    private async Task CloseWithError(WebSocket webSocket, string message)
    {
        try
        {
            if (webSocket.State == WebSocketState.Open)
            {
                var errorMessage = JsonSerializer.Serialize(new { error = message });
                var bytes = Encoding.UTF8.GetBytes(errorMessage);
                await webSocket.SendAsync(
                    new ArraySegment<byte>(bytes),
                    WebSocketMessageType.Text,
                    true,
                    CancellationToken.None
                );

                await webSocket.CloseAsync(
                    WebSocketCloseStatus.PolicyViolation,
                    message,
                    CancellationToken.None
                );
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error closing WebSocket");
        }
    }

    private async Task SendWarningAsync(WebSocket webSocket, string message)
    {
        try
        {
            if (webSocket.State == WebSocketState.Open)
            {
                var warning = JsonSerializer.Serialize(new { type = "warning", message });
                var bytes = Encoding.UTF8.GetBytes(warning);
                await webSocket.SendAsync(
                    new ArraySegment<byte>(bytes),
                    WebSocketMessageType.Text,
                    true,
                    CancellationToken.None
                );
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error sending warning");
        }
    }
}
