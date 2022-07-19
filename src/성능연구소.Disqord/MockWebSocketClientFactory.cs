using Disqord.Gateway.Api.Models;
using Disqord.Serialization.Json;
using Disqord.WebSocket;

namespace 성능연구소.Disqord;

public class MockWebSocketClientFactory : IWebSocketClientFactory
{
    private readonly IJsonSerializer _serializer;
    private readonly MockWebSocketClient _client;

    public MockWebSocketClientFactory(IJsonSerializer serializer)
    {
        _serializer = serializer;
        _client = new MockWebSocketClient(serializer);
    }

    public void SetCts(CancellationTokenSource cts) => _client.Cts = cts;

    public IWebSocketClient CreateClient()
    {
        return _client;
    }
}

public class MockWebSocketClient : IWebSocketClient
{
    private readonly IJsonSerializer _serializer;
    public CancellationTokenSource Cts { get; set; } = null!;

    public void Dispose()
    {
    }

    public Task ConnectAsync(Uri uri, CancellationToken cancellationToken = new CancellationToken())
    {
        State = WebSocketState.Open;
        return Task.CompletedTask;
    }

    public Task CloseAsync(int closeStatus, string closeMessage,
        CancellationToken cancellationToken = new CancellationToken())
    {
        State = WebSocketState.Closed;
        return Task.CompletedTask;
    }

    public Task CloseOutputAsync(int closeStatus, string closeMessage,
        CancellationToken cancellationToken = new CancellationToken())
    {
        State = WebSocketState.Closed;
        return Task.CompletedTask;
    }

    private int _eventIndex = 0;
    private int _eventPos = 0;

    public MockWebSocketClient(IJsonSerializer serializer)
    {
        _serializer = serializer;
    }

    public ValueTask<WebSocketResult> ReceiveAsync(Memory<byte> buffer,
        CancellationToken cancellationToken = new())
    {
        var messageType = WebSocketMessageType.Text;

        if (_eventIndex > 2)
        {
            _eventIndex = 0;
            Cts.Cancel();
            return new(new WebSocketResult(0, WebSocketMessageType.Close, true));
        }

        var gwEvent = MockGatewayMessage.Events[_eventIndex];
        if (gwEvent.Length - _eventPos > buffer.Length)
        {
            gwEvent.AsMemory()
                .Slice(_eventPos, buffer.Length)
                .CopyTo(buffer);

            _eventPos += buffer.Length;
            return new(new WebSocketResult(buffer.Length, messageType, false));
        }

        var finalSegment = gwEvent.AsMemory().Slice(_eventPos, gwEvent.Length - _eventPos);
        finalSegment.CopyTo(buffer);

        _eventPos = 0;
        _eventIndex++;
        return new(new WebSocketResult(finalSegment.Length, messageType, true));
    }

    public ValueTask SendAsync(ReadOnlyMemory<byte> buffer, WebSocketMessageType messageType, bool endOfMessage,
        CancellationToken cancellationToken = new CancellationToken())
    {
        return new ValueTask();
    }

    public WebSocketState State { get; private set; }
    public int? CloseStatus { get; }
    public string? CloseMessage { get; }
}