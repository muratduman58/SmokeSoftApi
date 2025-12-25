using System.Collections.Concurrent;
using System.Net.WebSockets;
using System.Text;
using System.Text.Json;
using SmokeSoft.Shared.DTOs.ShadowGuard;

namespace SmokeSoft.Services.ShadowGuard.WebSockets;

public class VoiceWebSocketHandler
{
    private readonly WebSocketConnectionManager _connectionManager;
    private readonly ConcurrentDictionary<string, MemoryStream> _audioBuffers = new();

    public VoiceWebSocketHandler(WebSocketConnectionManager connectionManager)
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
            await SendStatusAsync(connectionId, new AudioStreamStatus
            {
                IsRecording = false,
                IsProcessing = false,
                Message = "Connected successfully. Ready to receive audio.",
                BytesReceived = 0
            });

            await ReceiveAudioAsync(socket, connectionId, userId, context.RequestAborted);
        }
        finally
        {
            // Cleanup audio buffer
            if (_audioBuffers.TryRemove(connectionId, out var buffer))
            {
                buffer?.Dispose();
            }

            _connectionManager.RemoveConnection(connectionId);

            if (socket.State == WebSocketState.Open)
            {
                await socket.CloseAsync(WebSocketCloseStatus.NormalClosure, "Connection closed", CancellationToken.None);
            }

            socket.Dispose();
        }
    }

    private async Task ReceiveAudioAsync(WebSocket socket, string connectionId, Guid userId, CancellationToken cancellationToken)
    {
        var buffer = new byte[1024 * 64]; // 64KB buffer for audio chunks
        var totalBytesReceived = 0;

        while (socket.State == WebSocketState.Open && !cancellationToken.IsCancellationRequested)
        {
            try
            {
                var result = await socket.ReceiveAsync(new ArraySegment<byte>(buffer), cancellationToken);

                if (result.MessageType == WebSocketMessageType.Close)
                {
                    break;
                }

                if (result.MessageType == WebSocketMessageType.Binary)
                {
                    // Handle binary audio data
                    await ProcessAudioDataAsync(connectionId, userId, buffer, result.Count, result.EndOfMessage, cancellationToken);
                    totalBytesReceived += result.Count;

                    // Send status update periodically
                    if (totalBytesReceived % (1024 * 256) == 0) // Every 256KB
                    {
                        await SendStatusAsync(connectionId, new AudioStreamStatus
                        {
                            IsRecording = true,
                            IsProcessing = false,
                            Message = "Receiving audio...",
                            BytesReceived = totalBytesReceived
                        });
                    }
                }
                else if (result.MessageType == WebSocketMessageType.Text)
                {
                    // Handle control messages (start/stop recording, etc.)
                    var message = Encoding.UTF8.GetString(buffer, 0, result.Count);
                    await ProcessControlMessageAsync(connectionId, userId, message, cancellationToken);
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

    private async Task ProcessAudioDataAsync(string connectionId, Guid userId, byte[] buffer, int count, bool endOfMessage, CancellationToken cancellationToken)
    {
        try
        {
            // Get or create audio buffer for this connection
            var audioBuffer = _audioBuffers.GetOrAdd(connectionId, _ => new MemoryStream());

            // Append audio data
            await audioBuffer.WriteAsync(buffer.AsMemory(0, count), cancellationToken);

            if (endOfMessage)
            {
                // Complete audio chunk received
                var audioData = audioBuffer.ToArray();
                audioBuffer.SetLength(0); // Clear buffer for next chunk

                // Here you would process the audio (speech-to-text, AI processing, etc.)
                // For now, we'll just echo back a status
                await SendStatusAsync(connectionId, new AudioStreamStatus
                {
                    IsRecording = true,
                    IsProcessing = true,
                    Message = $"Processing audio chunk ({audioData.Length} bytes)...",
                    BytesReceived = audioData.Length
                });

                // TODO: Implement actual audio processing
                // - Speech-to-text conversion
                // - AI response generation
                // - Text-to-speech for AI response
                // - Send audio response back to client

                // Simulate processing complete
                await Task.Delay(100, cancellationToken);

                await SendStatusAsync(connectionId, new AudioStreamStatus
                {
                    IsRecording = true,
                    IsProcessing = false,
                    Message = "Audio chunk processed successfully",
                    BytesReceived = audioData.Length
                });
            }
        }
        catch (Exception ex)
        {
            await SendErrorAsync(connectionId, $"Error processing audio: {ex.Message}");
        }
    }

    private async Task ProcessControlMessageAsync(string connectionId, Guid userId, string message, CancellationToken cancellationToken)
    {
        try
        {
            var wsMessage = JsonSerializer.Deserialize<VoiceWebSocketMessage>(message);

            if (wsMessage == null)
                return;

            switch (wsMessage.Type.ToLower())
            {
                case "start_recording":
                    // Client started recording
                    if (_audioBuffers.TryGetValue(connectionId, out var existingBuffer))
                    {
                        existingBuffer.SetLength(0); // Clear any existing data
                    }
                    else
                    {
                        _audioBuffers.TryAdd(connectionId, new MemoryStream());
                    }

                    await SendStatusAsync(connectionId, new AudioStreamStatus
                    {
                        IsRecording = true,
                        IsProcessing = false,
                        Message = "Recording started. Send audio data.",
                        BytesReceived = 0
                    });
                    break;

                case "stop_recording":
                    // Client stopped recording
                    await SendStatusAsync(connectionId, new AudioStreamStatus
                    {
                        IsRecording = false,
                        IsProcessing = false,
                        Message = "Recording stopped.",
                        BytesReceived = 0
                    });
                    break;

                case "ping":
                    // Keep-alive ping
                    await SendMessageAsync(connectionId, new VoiceWebSocketMessage
                    {
                        Type = "pong",
                        Data = new { timestamp = DateTime.UtcNow }
                    }, cancellationToken);
                    break;

                default:
                    await SendErrorAsync(connectionId, "Unknown message type");
                    break;
            }
        }
        catch (JsonException)
        {
            await SendErrorAsync(connectionId, "Invalid message format");
        }
    }

    private async Task SendStatusAsync(string connectionId, AudioStreamStatus status)
    {
        await SendMessageAsync(connectionId, new VoiceWebSocketMessage
        {
            Type = "status",
            Data = status
        });
    }

    private async Task SendErrorAsync(string connectionId, string errorMessage)
    {
        await SendMessageAsync(connectionId, new VoiceWebSocketMessage
        {
            Type = "error",
            Data = new { message = errorMessage }
        });
    }

    private async Task SendMessageAsync(string connectionId, VoiceWebSocketMessage message, CancellationToken cancellationToken = default)
    {
        var socket = _connectionManager.GetConnection(connectionId);
        if (socket?.State == WebSocketState.Open)
        {
            var json = JsonSerializer.Serialize(message);
            var bytes = Encoding.UTF8.GetBytes(json);
            await socket.SendAsync(new ArraySegment<byte>(bytes), WebSocketMessageType.Text, true, cancellationToken);
        }
    }
}
