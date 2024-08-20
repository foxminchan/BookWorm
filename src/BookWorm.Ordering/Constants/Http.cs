namespace BookWorm.Ordering.Constants;

public static class Http
{
    public const string Idempotency = "X-Idempotency-Key";

    public static class Methods
    {
        public const string Post = "POST";
        public const string Patch = "PATCH";
    }
}
