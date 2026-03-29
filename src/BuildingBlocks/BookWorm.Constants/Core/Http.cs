namespace BookWorm.Constants.Core;

public static class Http
{
    public const string RequestIdHeader = "x-request-id";

    public static class Endpoints
    {
        public const string DevUIEndpointPath = "/devui";
        public const string HealthEndpointPath = "/health";
        public const string AlivenessEndpointPath = "/alive";
        public const string QuartzDashboardEndpointPath = "/quartz";
    }

    public static class Schemes
    {
        public static readonly string HttpOrHttps = $"{Uri.UriSchemeHttps}+{Uri.UriSchemeHttp}";
    }
}
