using System.Diagnostics;
using System.Text.RegularExpressions;
using OpenTelemetry;
using OpenTelemetry.Trace;

namespace BookWorm.Chassis.OpenTelemetry;

internal sealed partial class FixHttpRouteProcessor : BaseProcessor<Activity>
{
    private const string HttpRequestMethodTag = "http.request.method";
    private const string UrlPathTag = "url.path";
    private const string HttpRouteTag = "http.route";
    private const string NameTag = "name";
    private const string RequestNameTag = "request.name";

    [GeneratedRegex(@"\{version:apiVersion\}", RegexOptions.IgnoreCase)]
    private static partial Regex ApiVersionTokenRegex();

    [GeneratedRegex(
        @"/v(?<version>\d+(?:\.\d+)?|\d{4}-\d{2}-\d{2})(?=/|$)",
        RegexOptions.IgnoreCase
    )]
    private static partial Regex ApiVersionSegmentRegex();

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

        var existingRoute = activity.GetTagItem(HttpRouteTag)?.ToString();
        string targetRoute;

        if (!string.IsNullOrWhiteSpace(existingRoute))
        {
            if (!ApiVersionTokenRegex().IsMatch(existingRoute))
            {
                return;
            }

            var versionMatch = ApiVersionSegmentRegex().Match(path);
            if (!versionMatch.Success)
            {
                return;
            }

            targetRoute = ApiVersionTokenRegex()
                .Replace(existingRoute, versionMatch.Groups["version"].Value);
        }
        else
        {
            targetRoute = path;
        }

        var displayName = $"{method} {targetRoute}";
        activity.DisplayName = displayName;
        activity.SetTag(HttpRouteTag, targetRoute);
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
