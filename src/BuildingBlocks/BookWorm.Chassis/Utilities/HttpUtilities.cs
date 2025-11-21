namespace BookWorm.Chassis.Utilities;

public static class HttpUtilities
{
    public static string BuildUrl(string scheme, string host, int? port = null)
    {
        return port.HasValue ? $"{scheme}://{host}:{port.Value}" : $"{scheme}://{host}";
    }
}
