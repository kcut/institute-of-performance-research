using System.Globalization;
using System.Net;
using System.Text;

namespace 성능연구소;

public static class MockHttpResponseMessage
{
    private static readonly Version HttpVersion = Version.Parse("1.1");

    private static readonly string ResetValue = DateTimeOffset.UtcNow.AddHours(1).ToUnixTimeSeconds().ToString();

    private static readonly string ResetAfterValue = (DateTimeOffset.UtcNow.AddHours(1) - DateTimeOffset.UtcNow)
        .TotalMilliseconds
        .ToString(CultureInfo.InvariantCulture);

    private static string _sampleJson = "{}";

    public enum SampleJsonType
    {
        EmptyObject,
        EmptyArray,
        AllChannelsLarge,
        GatewayBot
    }
    
    public static void SelectJson(SampleJsonType jsonType)
    {
        _sampleJson = jsonType switch
        {
            SampleJsonType.EmptyObject => "{}",
            SampleJsonType.EmptyArray => "[]",
            SampleJsonType.AllChannelsLarge => File.ReadAllText("channels_list.json"),
            SampleJsonType.GatewayBot => File.ReadAllText("gateway_bot.json"),
            _ => throw new ArgumentOutOfRangeException(nameof(jsonType), jsonType, "Invalid JSON type")
        };
    }
    
    public static HttpResponseMessage CreateResponseMessage()
    {
        var response = new HttpResponseMessage()
        {
            Content = new StringContent(_sampleJson, Encoding.UTF8, "application/json"),
            StatusCode = HttpStatusCode.OK,
            Version = HttpVersion
        };
        
        response.Headers.Add("X-RateLimit-Bucket", "abcdefghijklmnop");
        response.Headers.Add("X-RateLimit-Remaining", "4");
        response.Headers.Add("X-RateLimit-Limit", "5");
        response.Headers.Add("X-RateLimit-Reset", ResetValue);
        response.Headers.Add("X-RateLimit-Reset-After", ResetAfterValue);

        return response;
    }
}