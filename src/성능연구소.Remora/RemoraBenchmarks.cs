using System.Reflection;
using System.Text.Json;
using BenchmarkDotNet.Attributes;
using HarmonyLib;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Options;
using Remora.Discord.API.Abstractions.Rest;
using Remora.Discord.API.Objects;
using Remora.Discord.Gateway;
using Remora.Discord.Gateway.Extensions;
using Remora.Rest;
using Remora.Rest.Core;

namespace 성능연구소.Remora;

[MemoryDiagnoser]
[SimpleJob]
public class RemoraBenchmarks
{
    private IDiscordRestGuildAPI _api = null!;
    private DiscordGatewayClient _gateway = null!;

    static RemoraBenchmarks()
    {
        var harmony = new Harmony("kp.net.kut");
        harmony.PatchAll(Assembly.GetExecutingAssembly());
    }
    
    private void GlobalSetup()
    {
        var services = new ServiceCollection();
        // services.AddDiscordRest(x => "a.b.c");
        services.AddDiscordGateway(x => "a.b.c");
        services.AddResponder<FakeResponder>();
        services.Replace(ServiceDescriptor.Transient(s =>
        {
            var client = new HttpClient(new MockHttpHandler());
            var options = s.GetRequiredService<IOptionsMonitor<JsonSerializerOptions>>().Get("Discord");

            client.BaseAddress = new Uri("https://example.org/");
            return new RestHttpClient<RestError>(client, options);
        }));

        var provider = services.BuildServiceProvider();
        _api = provider.GetRequiredService<IDiscordRestGuildAPI>();
        _gateway = provider.GetRequiredService<DiscordGatewayClient>();
    }

    [GlobalSetup(Target = nameof(RestApiCallLargeBodyLowLevel))]
    public void GlobalSetupLargeBody()
    {
        GlobalSetup();
        MockHttpResponseMessage.SelectJson(MockHttpResponseMessage.SampleJsonType.AllChannelsLarge);
    }
    
    [Benchmark]
    public Task RestApiCallLargeBodyLowLevel()
    {
        return _api.GetGuildChannelsAsync(new Snowflake(1234));
    }
    
    [GlobalSetup(Target = nameof(RestApiCallEmptyBodyLowLevel))]
    public void GlobalSetupEmptyBody()
    {
        GlobalSetup();
        MockHttpResponseMessage.SelectJson(MockHttpResponseMessage.SampleJsonType.EmptyArray);
    }
    
    [Benchmark]
    public Task RestApiCallEmptyBodyLowLevel()
    {
        return _api.GetGuildChannelsAsync(new Snowflake(1234));
    }

    [GlobalSetup(Target = nameof(WebSocketLifecycleLargeGuildCreate))]
    public void GlobalSetupLargeGuildCreate()
    {
        MockHttpResponseMessage.SelectJson(MockHttpResponseMessage.SampleJsonType.GatewayBot);
        MockGatewayMessage.SelectMessageType(MockGatewayMessage.MessageType.GuildCreateLarge);
    }
    
    [GlobalSetup(Target = nameof(WebSocketLifecycleSmallGuildCreate))]
    public void GlobalSetupSmallGuildCreate()
    {
        MockHttpResponseMessage.SelectJson(MockHttpResponseMessage.SampleJsonType.GatewayBot);
        MockGatewayMessage.SelectMessageType(MockGatewayMessage.MessageType.GuildCreateSmall);
    }

    [IterationSetup(Targets = new[]
    {
        nameof(WebSocketLifecycleLargeGuildCreate),
        nameof(WebSocketLifecycleSmallGuildCreate)
    })]
    public void WebSocketIterationSetup()
    {
        // library cannot handle cancellation proper
        // so must make new instance every benchmark run.
        // (ಠ_ಠ)
        GlobalSetup();
        ClientWebSocketReceiveAsyncPatch.Cts = new CancellationTokenSource();
    }

    [Benchmark]
    public async Task WebSocketLifecycleLargeGuildCreate()
    {
        try
        {
            await _gateway.RunAsync(ClientWebSocketReceiveAsyncPatch.Cts.Token);
        }
        catch
        {
            // ignored
        }
    }
    
    [Benchmark]
    public async Task WebSocketLifecycleSmallGuildCreate()
    {
        try
        {
            await _gateway.RunAsync(ClientWebSocketReceiveAsyncPatch.Cts.Token);
        }
        catch
        {
            // ignored
        }
    }
    
    [IterationCleanup(Targets = new[]
    {
        nameof(WebSocketLifecycleLargeGuildCreate),
        nameof(WebSocketLifecycleSmallGuildCreate)
    })]
    public void WebSocketIterationCleanup()
    {
        _gateway.Dispose();
        ClientWebSocketReceiveAsyncPatch.Cts.Dispose();
    }
}