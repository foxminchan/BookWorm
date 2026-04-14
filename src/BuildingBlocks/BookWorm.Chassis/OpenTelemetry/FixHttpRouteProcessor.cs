using System.Diagnostics;
using OpenTelemetry;
using OpenTelemetry.Trace;

namespace BookWorm.Chassis.OpenTelemetry;

internal sealed class FixHttpRouteProcessor : BaseProcessor<Activity>
{
    private const string HttpRequestMethodTag = "http.request.method";
    private const string UrlPathTag = "url.path";
    private const string HttpRouteTag = "http.route";
    private const string NameTag = "name";
    private const string RequestNameTag = "request.name";

    public override void OnEnd(Activity activity)
    {
        if (activity.Kind != ActivityKind.Server)
        {
            return;
        }

        var method = activity.GetTagItem(HttpRequestMethodTag)?.ToString();
        var path = activity.GetTagItem(UrlPathTag)?.ToString();

        if (string.IsNullOrWhiteSpace(method) || string.IsNullOrWhiteSpace(path))
        {
            return;
        }

        var displayName = $"{method} {path}";
        activity.DisplayName = displayName;
        activity.SetTag(HttpRouteTag, path);
        activity.SetTag(NameTag, displayName);
        activity.SetTag(RequestNameTag, displayName);
    }
}

public static class FixHttpRouteProcessorExtensions
{
    extension(TracerProviderBuilder tracerProviderBuilder)
    {
        /// <summary>
        ///     Adds the <see cref="FixHttpRouteProcessor" /> to the <see cref="TracerProviderBuilder" /> pipeline.
        /// </summary>
        /// <returns>
        ///     The <see cref="TracerProviderBuilder" /> with the <see cref="FixHttpRouteProcessor" /> registered,
        ///     enabling HTTP route normalization in OpenTelemetry traces.
        /// </returns>
        /// <example>
        ///     <code>
        ///         builder.Services.AddOpenTelemetry()
        ///             .WithTracing(tracing => tracing.AddFixHttpRouteProcessor());
        ///     </code>
        /// </example>
        public TracerProviderBuilder AddFixHttpRouteProcessor()
        {
            return tracerProviderBuilder.AddProcessor(new FixHttpRouteProcessor());
        }
    }
}
