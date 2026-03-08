using System.Diagnostics;

namespace BookWorm.SharedKernel.Helpers;

public static class DateTimeHelper
{
    public const string SqlUtcNow = "NOW() AT TIME ZONE 'UTC'";

    [DebuggerStepThrough]
    public static DateTime UtcNow()
    {
        return DateTimeOffset.Now.UtcDateTime.ToDateTime(DateTimeKind.Utc);
    }

    private static DateTime ToDateTime(this DateTime dateTime, DateTimeKind kind)
    {
        return DateTime.SpecifyKind(dateTime, kind);
    }
}
