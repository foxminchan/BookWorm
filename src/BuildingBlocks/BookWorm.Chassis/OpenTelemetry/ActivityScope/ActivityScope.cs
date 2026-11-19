using System.Diagnostics;

namespace BookWorm.Chassis.OpenTelemetry.ActivityScope;

internal sealed class ActivityScope : IActivityScope
{
    public Activity? Start(string name, StartActivityOptions options)
    {
        if (!ActivitySourceProvider.Instance.HasListeners())
        {
            return null;
        }

        return options.Parent.HasValue
            ? ActivitySourceProvider
                .Instance.CreateActivity(
                    $"{ActivitySourceProvider.DefaultSourceName}.{name}",
                    StartActivityOptions.Kind,
                    options.Parent.Value,
                    idFormat: ActivityIdFormat.W3C,
                    tags: options.Tags
                )
                ?.Start()
            : ActivitySourceProvider
                .Instance.CreateActivity(
                    $"{ActivitySourceProvider.DefaultSourceName}.{name}",
                    StartActivityOptions.Kind,
                    options.ParentId ?? Activity.Current?.ParentId,
                    idFormat: ActivityIdFormat.W3C,
                    tags: options.Tags
                )
                ?.Start();
    }

    public async Task Run(
        string name,
        Func<Activity?, CancellationToken, Task> run,
        StartActivityOptions options,
        CancellationToken ct
    )
    {
        using var activity = Start(name, options) ?? Activity.Current;

        try
        {
            await run(activity, ct);

            activity?.SetStatus(ActivityStatusCode.Ok);
        }
        catch (Exception ex)
        {
            RecordException(activity, ex);
            throw;
        }
    }

    public async Task<TResult> Run<TResult>(
        string name,
        Func<Activity?, CancellationToken, Task<TResult>> run,
        StartActivityOptions options,
        CancellationToken ct
    )
    {
        using var activity = Start(name, options) ?? Activity.Current;

        try
        {
            var result = await run(activity, ct);

            activity?.SetStatus(ActivityStatusCode.Ok);

            return result;
        }
        catch (Exception ex)
        {
            RecordException(activity, ex);
            throw;
        }
    }

    private static void RecordException(Activity? activity, Exception ex)
    {
        if (activity is null)
        {
            return;
        }

        activity.SetStatus(ActivityStatusCode.Error, ex.Message);
        activity.AddException(ex);
    }
}
