using System.Net;
using System.Reflection;
using BenchmarkDotNet.Attributes;
using DSharpPlus.Entities;
using DSharpPlus.Net;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using 성능연구소.Remora;

namespace 성능연구소.DSharpPlus;

[MemoryDiagnoser]
public class DSharpPlusBenchmarks
{
    private Func<ulong, Task<IReadOnlyList<DiscordChannel>>> _getGuildChannelsAsync = null!;
    
    private void GlobalSetup()
    {
        // very good design
        var ctor = typeof(DiscordApiClient).GetConstructor(
            BindingFlags.Instance | BindingFlags.NonPublic,
            new [] { typeof(IWebProxy), typeof(TimeSpan), typeof(bool), typeof(ILogger) }
        );

        var apiClient = (DiscordApiClient) ctor!.Invoke(new object[] { null!, TimeSpan.FromSeconds(100), false, NullLogger.Instance });

        var restClientProperty = typeof(DiscordApiClient).GetProperty("Rest", BindingFlags.Instance | BindingFlags.NonPublic);

        var restClient = restClientProperty!.GetValue(apiClient)!;
        restClient.GetType()
            .GetField("<HttpClient>k__BackingField", BindingFlags.Instance | BindingFlags.NonPublic)!
            .SetValue(restClient, new HttpClient(new MockHttpHandler()));
        
        _getGuildChannelsAsync = typeof(DiscordApiClient).GetMethod(
            "GetGuildChannelsAsync", 
            BindingFlags.Instance | BindingFlags.NonPublic
        )!.CreateDelegate<Func<ulong, Task<IReadOnlyList<DiscordChannel>>>>(apiClient);
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
        return _getGuildChannelsAsync(1234);
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
        return _getGuildChannelsAsync(1234);
    }
}