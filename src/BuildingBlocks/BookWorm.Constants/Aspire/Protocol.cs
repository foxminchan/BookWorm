namespace BookWorm.Constants.Aspire;

public static class Protocol
{
    public static readonly string Http = nameof(Http).ToLowerInvariant();
    public static readonly string Https = nameof(Https).ToLowerInvariant();
    public static readonly string HttpOrHttps = $"{Https}+{Http}";
    public static readonly string Tcp = nameof(Tcp).ToLowerInvariant();
    public static readonly string Otlp = nameof(Otlp).ToLowerInvariant();
    public static readonly string Grpc = nameof(Grpc).ToLowerInvariant();
}
