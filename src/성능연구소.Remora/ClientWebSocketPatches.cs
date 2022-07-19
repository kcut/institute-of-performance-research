using System.Net.WebSockets;
using HarmonyLib;

namespace 성능연구소.Remora;

// Remore very good abstract. Hard depend on Client Web Socket so benchmark must re-implement
// deserialize stack to fair comparison. Instead, patch ClientWeb Socket.
[HarmonyPatch(
    typeof(ClientWebSocket), 
    nameof(ClientWebSocket.ReceiveAsync), 
    typeof(ArraySegment<byte>), typeof(CancellationToken)
)]
public class ClientWebSocketReceiveAsyncPatch
{
    public static CancellationTokenSource Cts;
    
    private static int _eventIndex = 0;
    private static int _eventPos = 0;
    
    // ReSharper disable once InconsistentNaming
    static bool Prefix(ref Task<WebSocketReceiveResult> __result, ArraySegment<byte> buffer, CancellationToken cancellationToken)
    {
        if (cancellationToken.IsCancellationRequested)
        {
            throw new OperationCanceledException();
        }

        var messageType = WebSocketMessageType.Text;
        if (_eventIndex > 2)
        {
            _eventIndex = 0;
            __result = Task.Delay(-1, Cts.Token).ContinueWith(x => new WebSocketReceiveResult(0,
                WebSocketMessageType.Close,
                true,
                WebSocketCloseStatus.NormalClosure,
                null), Cts.Token);
            return false;
        }

        var gwEvent = MockGatewayMessage.Events[_eventIndex];
        if (gwEvent.Length - _eventPos > buffer.Count)
        {
            gwEvent.AsMemory()
                .Slice(_eventPos, buffer.Count)
                .CopyTo(buffer);

            _eventPos += buffer.Count;
            __result = Task.FromResult(new WebSocketReceiveResult(buffer.Count, messageType, false));
            return false;
        }
        
        var finalSegment = gwEvent.AsMemory().Slice(_eventPos, gwEvent.Length - _eventPos);
        finalSegment.CopyTo(buffer);

        _eventPos = 0;
        _eventIndex++;
        __result = Task.FromResult(new WebSocketReceiveResult(finalSegment.Length, WebSocketMessageType.Text, true));

        return false;
    }
}

[HarmonyPatch(
    typeof(ClientWebSocket),
    nameof(ClientWebSocket.SendAsync), 
    typeof(ArraySegment<byte>), typeof(WebSocketMessageType), typeof(bool), typeof(CancellationToken)
)]
public class ClientWebSocketSendAsyncPatch
{
    // ReSharper disable once InconsistentNaming
    static bool Prefix(ref Task __result, ArraySegment<byte> buffer, WebSocketMessageType messageType, bool endOfMessage, CancellationToken cancellationToken)
    {
        if (cancellationToken.IsCancellationRequested)
        {
            throw new OperationCanceledException();
        }
        
        __result = Task.CompletedTask;
        return false;
    }
}

[HarmonyPatch(
    typeof(ClientWebSocket),
    nameof(ClientWebSocket.ConnectAsync), 
    typeof(Uri), typeof(CancellationToken)
)]
public class ClientWebSocketConnectAsyncPatch
{
    // ReSharper disable once InconsistentNaming
    static bool Prefix(ref Task __result, Uri uri, CancellationToken cancellationToken)
    {
        if (cancellationToken.IsCancellationRequested)
        {
            throw new OperationCanceledException();
        }

        ClientWebSocketStateGetterPatch.State = WebSocketState.Open;
        __result = Task.CompletedTask;
        return false;
    }
}

[HarmonyPatch(
    typeof(ClientWebSocket),
    nameof(ClientWebSocket.State), 
    MethodType.Getter
)]
public class ClientWebSocketStateGetterPatch
{
    public static WebSocketState State = WebSocketState.None;
    
    // ReSharper disable once InconsistentNaming
    static bool Prefix(ref WebSocketState __result)
    {
        __result = State;
        return false;
    }
}

[HarmonyPatch(
    typeof(ClientWebSocket),
    nameof(ClientWebSocket.CloseAsync), 
    typeof(WebSocketCloseStatus), typeof(string), typeof(CancellationToken)
)]
public class ClientWebSocketCloseAsyncPatch
{
    // ReSharper disable once InconsistentNaming
    static bool Prefix(ref Task __result, WebSocketCloseStatus closeStatus, string statusDescription, CancellationToken cancellationToken)
    {
        ClientWebSocketStateGetterPatch.State = WebSocketState.Closed;
        __result = Task.CompletedTask;
        return false;
    }
}