using System.Net.WebSockets;
using System.Text;
using System.Text.Json;
using SmokeSoft.Shared.DTOs.ShadowGuard;

namespace SmokeSoft.Services.ShadowGuard.WebSockets;

public class WebSocketHandler
{
    private readonly WebSocketConnectionManager _connectionManager;

    public WebSocketHandler(WebSocketConnectionManager connectionManager)
    {
        _connectionManager = connectionManager;
    }

    public async Task HandleWebSocketAsync(HttpContext context, Guid userId)
    {
        if (!context.WebSockets.IsWebSocketRequest)
        {
            context.Response.StatusCode = 400;
            return;
        }

        var socket = await context.WebSockets.AcceptWebSocketAsync();
        var connectionId = $"{userId}_{Guid.NewGuid()}";

        _connectionManager.AddConnection(connectionId, socket);

        try
        {
            // Send connection status
            await _connectionManager.SendMessageAsync(connectionId, new WebSocketMessage
            {
                Type = "status",
                Data = new ConnectionStatus
                {
                    IsConnected = true,
                    Message = "Connected successfully"
                }
            });

            await ReceiveMessagesAsync(socket, connectionId, userId, context.RequestAborted);
        }
        finally
        {
            _connectionManager.RemoveConnection(connectionId);

            if (socket.State == WebSocketState.Open)
            {
                await socket.CloseAsync(WebSocketCloseStatus.NormalClosure, "Connection closed", CancellationToken.None);
            }

            socket.Dispose();
        }
    }

    private async Task ReceiveMessagesAsync(WebSocket socket, string connectionId, Guid userId, CancellationToken cancellationToken)
    {
        var buffer = new byte[1024 * 4];

        while (socket.State == WebSocketState.Open && !cancellationToken.IsCancellationRequested)
        {
            try
            {
                var result = await socket.ReceiveAsync(new ArraySegment<byte>(buffer), cancellationToken);

                if (result.MessageType == WebSocketMessageType.Close)
                {
                    break;
                }

                if (result.MessageType == WebSocketMessageType.Text)
                {
                    var message = Encoding.UTF8.GetString(buffer, 0, result.Count);
                    await ProcessMessageAsync(connectionId, userId, message, cancellationToken);
                }
            }
            catch (WebSocketException)
            {
                break;
            }
            catch (OperationCanceledException)
            {
                break;
            }
        }
    }

    private async Task ProcessMessageAsync(string connectionId, Guid userId, string message, CancellationToken cancellationToken)
    {
        try
        {
            var wsMessage = JsonSerializer.Deserialize<WebSocketMessage>(message);

            if (wsMessage == null)
                return;

            switch (wsMessage.Type.ToLower())
            {
                case "message":
                    // Handle chat message
                    if (wsMessage.Data != null)
                    {
                        var chatMessage = JsonSerializer.Deserialize<ChatMessage>(wsMessage.Data.ToString() ?? "{}");
                        if (chatMessage != null)
                        {
                            // Echo back the message (in real implementation, this would process through AI)
                            await _connectionManager.SendMessageAsync(connectionId, new WebSocketMessage
                            {
                                Type = "message",
                                Data = chatMessage
                            }, cancellationToken);
                        }
                    }
                    break;

                case "typing":
                    // Handle typing indicator
                    if (wsMessage.Data != null)
                    {
                        var typingIndicator = JsonSerializer.Deserialize<TypingIndicator>(wsMessage.Data.ToString() ?? "{}");
                        if (typingIndicator != null)
                        {
                            await _connectionManager.SendMessageAsync(connectionId, new WebSocketMessage
                            {
                                Type = "typing",
                                Data = typingIndicator
                            }, cancellationToken);
                        }
                    }
                    break;

                default:
                    // Unknown message type
                    await _connectionManager.SendMessageAsync(connectionId, new WebSocketMessage
                    {
                        Type = "error",
                        Data = new { message = "Unknown message type" }
                    }, cancellationToken);
                    break;
            }
        }
        catch (JsonException)
        {
            await _connectionManager.SendMessageAsync(connectionId, new WebSocketMessage
            {
                Type = "error",
                Data = new { message = "Invalid message format" }
            }, cancellationToken);
        }
    }
}
