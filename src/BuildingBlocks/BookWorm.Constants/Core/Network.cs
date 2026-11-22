namespace BookWorm.Constants.Core;

public static class Network
{
    public const string Localhost = "localhost";

    public static bool IsLocalhostOrigin(string origin)
    {
        if (!Uri.TryCreate(origin, UriKind.Absolute, out var uri))
        {
            return false;
        }

        return uri.Host.Equals(Localhost, StringComparison.OrdinalIgnoreCase)
            || uri.Host.EndsWith($".{Localhost}", StringComparison.OrdinalIgnoreCase);
    }
}
