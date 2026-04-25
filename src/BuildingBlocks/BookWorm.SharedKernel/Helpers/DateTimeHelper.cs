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

    extension(DateTime dateTime)
    {
        private DateTime ToDateTime(DateTimeKind kind)
        {
            return DateTime.SpecifyKind(dateTime, kind);
        }
    }
}
