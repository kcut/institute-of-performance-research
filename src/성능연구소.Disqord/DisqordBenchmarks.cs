using BenchmarkDotNet.Attributes;
using Disqord;
using Disqord.Gateway;
using Disqord.Http;
using Disqord.Rest;
using Disqord.Rest.Api;
using Disqord.WebSocket;
using Microsoft.Extensions.DependencyInjection;

namespace 성능연구소.Disqord;

[MemoryDiagnoser]
public class DisqordBenchmarks
{
    private IRestClient _restClient = null!;
    private IRestApiClient _restApiClient = null!;
    private IGatewayClient _gateway = null!;
    private MockWebSocketClientFactory _webSocket = null!;

    private void GlobalSetup()
    {
        Console.WriteLine(nameof(GlobalSetup));

        var services = new ServiceCollection();
        services.AddSingleton<Token>(
            Token.Bot("MTExMTExMTExMTExMTExMTEx.aW5zdGl0dXRlIG9mIHBlcmZvcm1hbmNlIHJlc2VhcmNo.c"));
        services.AddLogging();
        services.AddRestClient();
        services.AddGatewayClient();
        services.AddSingleton<IHttpClientFactory>(new MockHttpClientProvider());
        services.AddSingleton<IWebSocketClientFactory, MockWebSocketClientFactory>();
        var provider = services.BuildServiceProvider();
        _restClient = provider.GetRequiredService<IRestClient>();
        _restApiClient = provider.GetRequiredService<IRestApiClient>();
        _gateway = provider.GetRequiredService<IGatewayClient>();
        _webSocket = (provider.GetRequiredService<IWebSocketClientFactory>() as MockWebSocketClientFactory)!;
    }

    [GlobalSetup(Targets = new[] { nameof(RestApiCallLargeBodyHighLevel), nameof(RestApiCallLargeBodyLowLevel) })]
    public void GlobalSetupLargeBody()
    {
        GlobalSetup();
        MockHttpResponseMessage.SelectJson(MockHttpResponseMessage.SampleJsonType.AllChannelsLarge);
    }

    [Benchmark]
    public Task RestApiCallLargeBodyHighLevel()
    {
        return _restClient.FetchChannelsAsync(new Snowflake(1234));
    }

    [Benchmark]
    public Task RestApiCallLargeBodyLowLevel()
    {
        return _restApiClient.FetchGuildChannelsAsync(new Snowflake(1234));
    }

    [GlobalSetup(Targets = new[] { nameof(RestApiCallEmptyBodyHighLevel), nameof(RestApiCallEmptyBodyLowLevel) })]
    public void GlobalSetupEmptyBody()
    {
        GlobalSetup();
        MockHttpResponseMessage.SelectJson(MockHttpResponseMessage.SampleJsonType.EmptyArray);
    }

    [Benchmark]
    public Task RestApiCallEmptyBodyHighLevel()
    {
        return _restClient.FetchChannelsAsync(new Snowflake(1234));
    }

    [Benchmark]
    public Task RestApiCallEmptyBodyLowLevel()
    {
        return _restApiClient.FetchGuildChannelsAsync(new Snowflake(1234));
    }

    [GlobalSetup(Target = nameof(WebSocketLifecycleLargeGuildCreate))]
    public void GlobalSetupWebSocketLargeGuildCreate()
    {
        GlobalSetup();
        MockHttpResponseMessage.SelectJson(MockHttpResponseMessage.SampleJsonType.EmptyObject);
        MockGatewayMessage.SelectMessageType(MockGatewayMessage.MessageType.GuildCreateLarge);
    }

    [GlobalSetup(Target = nameof(WebSocketLifecycleSmallGuildCreate))]
    public void GlobalSetupWebSocketSmallGuildCreate()
    {
        GlobalSetup();
        MockHttpResponseMessage.SelectJson(MockHttpResponseMessage.SampleJsonType.EmptyObject);
        MockGatewayMessage.SelectMessageType(MockGatewayMessage.MessageType.GuildCreateSmall);
    }

    private CancellationTokenSource _cts = new();

    [IterationSetup(Targets = new[]
    {
        nameof(WebSocketLifecycleLargeGuildCreate), nameof(WebSocketLifecycleSmallGuildCreate)
    })]
    public void IterationSetupWebSocket()
    {
        _cts = new CancellationTokenSource();
        _webSocket.SetCts(_cts);
    }

    [IterationCleanup(Targets = new[]
    {
        nameof(WebSocketLifecycleLargeGuildCreate), nameof(WebSocketLifecycleSmallGuildCreate)
    })]
    public void IterationCleanupWebSocket()
    {
        _cts.Dispose();
    }

    [Benchmark]
    public async Task WebSocketLifecycleLargeGuildCreate()
    {
        try
        {
            await _gateway.RunAsync(new Uri("wss://example.org", UriKind.Absolute), _cts.Token);
        }
        catch (OperationCanceledException)
        {
        }
    }

    [Benchmark]
    public async Task WebSocketLifecycleSmallGuildCreate()
    {
        try
        {
            await _gateway.RunAsync(new Uri("wss://example.org", UriKind.Absolute), _cts.Token);
        }
        catch (OperationCanceledException)
        {
        }
    }
}