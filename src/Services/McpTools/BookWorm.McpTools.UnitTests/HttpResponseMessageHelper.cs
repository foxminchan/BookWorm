using System.Net;

namespace BookWorm.McpTools.UnitTests;

internal static class HttpResponseMessageHelper
{
    internal static HttpResponseMessage CreateResponse(HttpStatusCode statusCode)
    {
        return new(statusCode) { RequestMessage = new HttpRequestMessage() };
    }
}
