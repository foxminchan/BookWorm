using System.Diagnostics;

internal static class ActivityExtensions
{
    public static void SetExceptionTags(this Activity? activity, Exception ex)
    {
        if (activity is null) return;

        activity.AddTag("exception.message", ex.Message);
        activity.AddTag("exception.stacktrace", ex.ToString());
        activity.AddTag("exception.type", ex.GetType().FullName);
        activity.SetStatus(ActivityStatusCode.Error);
    }
}
