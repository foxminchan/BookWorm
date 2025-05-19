namespace BookWorm.Constants.Core;

public static class Restful
{
    public const string RequestIdHeader = "x-request-id";

    public static class Methods
    {
        public static readonly string Post = nameof(Post).ToUpperInvariant();
        public static readonly string Patch = nameof(Patch).ToUpperInvariant();
        public static readonly string Put = nameof(Put).ToUpperInvariant();
        public static readonly string Delete = nameof(Delete).ToUpperInvariant();
        public static readonly string Get = nameof(Get).ToUpperInvariant();
        public static readonly string Options = nameof(Options).ToUpperInvariant();
    }

    public static class Host
    {
        public static readonly string Localhost = nameof(Localhost).ToLowerInvariant();
    }
}
