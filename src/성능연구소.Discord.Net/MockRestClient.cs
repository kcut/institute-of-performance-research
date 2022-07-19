using System.Net;
using Discord.Net.Rest;

namespace 성능연구소.Discord.Net;

public class MockRestClient : IRestClient
{
    public void Dispose() { }

    public void SetHeader(string key, string value) { }

    public void SetCancelToken(CancellationToken cancelToken) { }
    
    private readonly Dictionary<string, string> _headers = new();
    
    private RestResponse GetResponse()
    {
        var stream = MockHttpResponseMessage.CreateResponseMessage().Content.ReadAsStream();
        return new RestResponse(HttpStatusCode.OK, _headers, stream);
    }

    public Task<RestResponse> SendAsync(string method, string endpoint, CancellationToken cancelToken, bool headerOnly = false,
        string reason = null!)
    {
        return Task.FromResult(GetResponse());
    }

    public Task<RestResponse> SendAsync(string method, string endpoint, string json, CancellationToken cancelToken, bool headerOnly = false,
        string reason = null!)
    {
        return Task.FromResult(GetResponse());
    }

    public Task<RestResponse> SendAsync(string method, string endpoint, IReadOnlyDictionary<string, object> multipartParams, CancellationToken cancelToken,
        bool headerOnly = false, string reason = null!)
    {
        return Task.FromResult(GetResponse());
    }
}