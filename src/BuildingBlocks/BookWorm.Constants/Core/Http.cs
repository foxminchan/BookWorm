namespace BookWorm.Constants.Core;

public static class Http
{
    public const string RequestIdHeader = "x-request-id";

    public static class Methods
    {
        public const string Get = "GET";
        public const string Post = "POST";
        public const string Put = "PUT";
        public const string Patch = "PATCH";
        public const string Delete = "DELETE";
        public const string Head = "HEAD";
        public const string Options = "OPTIONS";
    }

    public static class Endpoints
    {
        public const string HealthEndpointPath = "/health";
    }

    public static class Schemes
    {
        public const string Http = "http";
        public const string Https = "https";
        public const string HttpOrHttps = "https+http";
    }
}
