using System.Text;
using Discord.Net.WebSockets;

namespace 성능연구소.Discord.Net;

public class MockWebSocketClient : IWebSocketClient
{
    public void Dispose() { }

    public void SetHeader(string key, string value) { }

    public void SetCancelToken(CancellationToken cancelToken) { }

    public Task ConnectAsync(string host) => Task.CompletedTask;

    public Task DisconnectAsync(int closeCode = 1000) => Task.CompletedTask;

    public Task SendAsync(byte[] data, int index, int count, bool isText) => Task.CompletedTask;

    public async Task TriggerEvents()
    {
        foreach (var message in MockGatewayMessage.Events)
        {
            await TextMessage?.Invoke(Encoding.UTF8.GetString(message))!;
        }

        Closed?.Invoke(new Exception("Closed."));
    }
    
    public event Func<byte[], int, int, Task>? BinaryMessage;
    public event Func<string, Task>? TextMessage;
    public event Func<Exception, Task>? Closed;
}