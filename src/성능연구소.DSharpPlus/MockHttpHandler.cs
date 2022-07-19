namespace 성능연구소.Remora;

public class MockHttpHandler : HttpClientHandler
{
    protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        return Task.FromResult(MockHttpResponseMessage.CreateResponseMessage());
    }
}