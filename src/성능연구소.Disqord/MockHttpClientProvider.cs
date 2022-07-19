using Disqord.Http;

namespace 성능연구소.Disqord;

public class MockHttpClientProvider : IHttpClientFactory
{
    public IHttpClient CreateClient()
    {
        return new MockHttpClient();
    }
}