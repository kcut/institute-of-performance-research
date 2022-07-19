using System.Reflection;
using BenchmarkDotNet.Attributes;
using Discord;
using Discord.Net.Rest;
using Discord.Rest;
using Discord.WebSocket;
using Newtonsoft.Json;

namespace 성능연구소.Discord.Net;

[MemoryDiagnoser]
public class DiscordNetBenchmarks
{
    private Func<ulong, RequestOptions, Task> _getGuildChannelsAsync = null!;

    private object CreateRestApiClient()
    {
        var clientType = Assembly.GetAssembly(typeof(DiscordRestClient))!.GetType("Discord.API.DiscordRestApiClient");
        var ctor = clientType!.GetConstructor(
            new[] { 
                typeof(RestClientProvider), typeof(string), typeof(RetryMode), typeof(JsonSerializer), 
                typeof(bool), typeof(Func<IRateLimitInfo, Task>) 
            }
        );

        return ctor!.Invoke(new object?[]
        {
            (RestClientProvider) (s => new MockRestClient()), "Institute of Performance Research", 
            RetryMode.AlwaysFail, null, true, null
        });
    }
    
    private async Task GlobalSetup()
    {
        var instance = CreateRestApiClient();

        var loginAsync = instance.GetType().GetMethod("LoginAsync")!
            .CreateDelegate<Func<TokenType, string, RequestOptions, Task>>(instance);

        await loginAsync(TokenType.Bot, "MTExMTExMTExMTExMTExMTEx.aW5zdGl0dXRlIG9mIHBlcmZvcm1hbmNlIHJlc2VhcmNo.c", RequestOptions.Default);
        
        _getGuildChannelsAsync = instance.GetType().GetMethod(
            "GetGuildChannelsAsync"
        )!.CreateDelegate<Func<ulong, RequestOptions, Task>>(instance);
    }
    
    [GlobalSetup(Target = nameof(RestApiCallLargeBodyLowLevel))]
    public async Task GlobalSetupLargeBody()
    {
        await GlobalSetup();
        MockHttpResponseMessage.SelectJson(MockHttpResponseMessage.SampleJsonType.AllChannelsLarge);
    }
    
    [Benchmark]
    public Task RestApiCallLargeBodyLowLevel()
    {
        return _getGuildChannelsAsync(1234, RequestOptions.Default);
    }
    
    [GlobalSetup(Target = nameof(RestApiCallEmptyBodyLowLevel))]
    public async Task GlobalSetupEmptyBody()
    {
        await GlobalSetup();
        MockHttpResponseMessage.SelectJson(MockHttpResponseMessage.SampleJsonType.EmptyArray);
    }
    
    [Benchmark]
    public Task RestApiCallEmptyBodyLowLevel()
    {
        return _getGuildChannelsAsync(1234, RequestOptions.Default);
    }

    private readonly MockWebSocketClient _webSocket = new();
    private DiscordSocketClient? _client = null!;

    private async Task GlobalSetupWebSocket()
    {
        await GlobalSetup();
        MockHttpResponseMessage.SelectJson(MockHttpResponseMessage.SampleJsonType.EmptyObject);
        _client = new DiscordSocketClient(new DiscordSocketConfig()
        {
            WebSocketProvider = () => _webSocket,
            RestClientProvider = (_) => new MockRestClient()
        });
    }
    
    [GlobalSetup(Target = nameof(WebSocketLifecycleLargeGuildCreate))]
    public async Task GlobalSetupWebSocketLargeGuildCreate()
    {
        await GlobalSetupWebSocket();
        MockGatewayMessage.SelectMessageType(MockGatewayMessage.MessageType.GuildCreateLarge);
    }
    
    [GlobalSetup(Target = nameof(WebSocketLifecycleSmallGuildCreate))]
    public async Task GlobalSetupWebSocketSmallGuildCreate()
    {
        await GlobalSetupWebSocket();
        MockGatewayMessage.SelectMessageType(MockGatewayMessage.MessageType.GuildCreateSmall);
    }
    
    [IterationSetup(Targets = new[]
    {
        nameof(WebSocketLifecycleLargeGuildCreate),
        nameof(WebSocketLifecycleSmallGuildCreate)
    })]
    public void IterationSetupWebSocket()
    {
        _client ??= new DiscordSocketClient(new DiscordSocketConfig()
        {
            WebSocketProvider = () => _webSocket,
            RestClientProvider = (_) => new MockRestClient()
        });
    }
    
    [Benchmark]
    public async Task WebSocketLifecycleLargeGuildCreate()
    {
        try
        {
            await _client.LoginAsync(TokenType.Bot,
                "MTExMTExMTExMTExMTExMTEx.aW5zdGl0dXRlIG9mIHBlcmZvcm1hbmNlIHJlc2VhcmNo.c");
            await Task.WhenAll(_client.StartAsync(), _webSocket.TriggerEvents());
            await _client.StopAsync();
            await _client.LogoutAsync();
        }
        catch
        {
            // discord net library race condition
            // client cannot reuse between benchmark some time as state not updated
            // we compensate bad code with workaround!
            _client.Dispose();
            _client = null;
        }
    }

    [Benchmark]
    public async Task WebSocketLifecycleSmallGuildCreate()
    {
        try
        {
            await _client.LoginAsync(TokenType.Bot,
                "MTExMTExMTExMTExMTExMTEx.aW5zdGl0dXRlIG9mIHBlcmZvcm1hbmNlIHJlc2VhcmNo.c");
            await Task.WhenAll(_client.StartAsync(), _webSocket.TriggerEvents());
            await _client.StopAsync();
            await _client.LogoutAsync();
        }
        catch
        {
            // discord net library race condition
            // client cannot reuse between benchmark some time as state not updated
            // we compensate bad code with workaround!
            _client.Dispose();
            _client = null;
        }
    }
}