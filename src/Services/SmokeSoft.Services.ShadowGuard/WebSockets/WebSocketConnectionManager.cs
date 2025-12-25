using System.Collections.Concurrent;
using System.Net.WebSockets;
using System.Text;
using System.Text.Json;
using SmokeSoft.Shared.DTOs.ShadowGuard;

namespace SmokeSoft.Services.ShadowGuard.WebSockets;

public class WebSocketConnectionManager
{
    private readonly ConcurrentDictionary<string, WebSocket> _connections = new();

    public void AddConnection(string connectionId, WebSocket socket)
    {
        _connections.TryAdd(connectionId, socket);
    }

    public void RemoveConnection(string connectionId)
    {
        _connections.TryRemove(connectionId, out _);
    }

    public WebSocket? GetConnection(string connectionId)
    {
        _connections.TryGetValue(connectionId, out var socket);
        return socket;
    }

    public IEnumerable<string> GetAllConnectionIds()
    {
        return _connections.Keys;
    }

    public async Task SendMessageAsync(string connectionId, WebSocketMessage message, CancellationToken cancellationToken = default)
    {
        var socket = GetConnection(connectionId);
        if (socket?.State == WebSocketState.Open)
        {
            var json = JsonSerializer.Serialize(message);
            var bytes = Encoding.UTF8.GetBytes(json);
            await socket.SendAsync(new ArraySegment<byte>(bytes), WebSocketMessageType.Text, true, cancellationToken);
        }
    }

    public async Task BroadcastMessageAsync(WebSocketMessage message, CancellationToken cancellationToken = default)
    {
        var tasks = _connections.Values
            .Where(s => s.State == WebSocketState.Open)
            .Select(async socket =>
            {
                var json = JsonSerializer.Serialize(message);
                var bytes = Encoding.UTF8.GetBytes(json);
                await socket.SendAsync(new ArraySegment<byte>(bytes), WebSocketMessageType.Text, true, cancellationToken);
            });

        await Task.WhenAll(tasks);
    }
}
