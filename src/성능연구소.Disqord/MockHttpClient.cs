using Disqord.Http;
using Disqord.Http.Default;

namespace 성능연구소.Disqord;

public class MockHttpClient : IHttpClient
{
    public Uri BaseUri { get; set; } = new("https://example.org");

    public void SetDefaultHeader(string name, string value) { }
    
    public void Dispose() { }
    
    public Task<IHttpResponse> SendAsync(IHttpRequest request, CancellationToken cancellationToken = new CancellationToken())
    {
        return Task.FromResult<IHttpResponse>(new DefaultHttpResponse(MockHttpResponseMessage.CreateResponseMessage()));
    }
}