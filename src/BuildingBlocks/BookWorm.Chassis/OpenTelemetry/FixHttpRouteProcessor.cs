using System.Diagnostics;
using OpenTelemetry;

namespace BookWorm.Chassis.OpenTelemetry;

public sealed class FixHttpRouteProcessor : BaseProcessor<Activity>
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
