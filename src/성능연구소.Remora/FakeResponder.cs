using Remora.Discord.API.Abstractions.Gateway.Events;
using Remora.Discord.Gateway.Responders;
using Remora.Results;

namespace 성능연구소.Remora;

public class FakeResponder : IResponder<IGuildCreate>
{
    public Task<Result> RespondAsync(IGuildCreate gatewayEvent, CancellationToken ct = new CancellationToken())
    {
        ClientWebSocketReceiveAsyncPatch.Cts.Cancel();
        return Task.FromResult(Result.FromSuccess());
    }
}