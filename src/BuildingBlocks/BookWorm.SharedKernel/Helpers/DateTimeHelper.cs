using System.Diagnostics;

namespace BookWorm.SharedKernel.Helpers;

public static class DateTimeHelper
{
    public const string SqlUtcNow = "NOW() AT TIME ZONE 'UTC'";

    [DebuggerStepThrough]
    public static DateTime UtcNow()
    {
        return ToDateTime(DateTimeOffset.Now.UtcDateTime, DateTimeKind.Utc);
    }

    private static DateTime ToDateTime(this DateTime dateTime, DateTimeKind kind)
    {
        return DateTime.SpecifyKind(dateTime, kind);
    }
}
