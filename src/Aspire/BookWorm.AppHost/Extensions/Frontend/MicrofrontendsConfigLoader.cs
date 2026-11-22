using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.RegularExpressions;

namespace BookWorm.AppHost.Extensions.Frontend;

internal sealed record MicrofrontendRouteDefinition(string ZoneName, string Pattern, bool IsExact);

internal static partial class MicrofrontendsConfigLoader
{
    private const string CatchAllSuffix = "{**remainder}";

    /// <summary>
    ///     Loads and translates microfrontend routing definitions from a Vercel-style microfrontends JSON file.
    /// </summary>
    /// <param name="configPath">Absolute or relative path to <c>microfrontends.json</c>.</param>
    /// <returns>The ordered list of YARP route definitions derived from the configuration.</returns>
    public static IReadOnlyList<MicrofrontendRouteDefinition> Load(string configPath)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(configPath);

        if (!File.Exists(configPath))
        {
            throw new FileNotFoundException(
                $"Microfrontends configuration file was not found at '{configPath}'.",
                configPath
            );
        }

        var json = File.ReadAllText(configPath);

        return Parse(json);
    }

    /// <summary>
    ///     Parses Vercel-style microfrontends JSON content into YARP route definitions.
    /// </summary>
    /// <param name="json">Raw JSON content of <c>microfrontends.json</c>.</param>
    /// <returns>The ordered list of YARP route definitions derived from the configuration.</returns>
    private static IReadOnlyList<MicrofrontendRouteDefinition> Parse(string json)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(json);

        var config = JsonSerializer.Deserialize(
            json,
            MicrofrontendsConfigSerializationContext.Default.MicrofrontendsConfig
        )
            ?? throw new InvalidOperationException(
                "Failed to deserialize microfrontends configuration."
            );

        if (config.Applications is null || config.Applications.Count == 0)
        {
            return [];
        }

        List<MicrofrontendRouteDefinition> routes = [];
        HashSet<(string ZoneName, string Pattern)> seen = [];

        foreach (var (appName, appConfig) in config.Applications)
        {
            if (appConfig is null)
            {
                continue;
            }

            appName.AddRoutingRoutes(appConfig.Routing, seen, routes);
            appName.AddAssetPrefixRoute(appConfig.AssetPrefix, seen, routes);
        }

        return routes;
    }

    extension(string appName)
    {
        private void AddRoutingRoutes(
            IReadOnlyList<MicrofrontendRoutingGroup>? routing,
            HashSet<(string ZoneName, string Pattern)> seen,
            List<MicrofrontendRouteDefinition> routes
        )
        {
            if (routing is null)
            {
                return;
            }

            foreach (var rawPath in routing.SelectMany(static group => group.Paths ?? []))
            {
                var (pattern, isExact) = TranslatePathPattern(rawPath);
                appName.TryAddRoute(pattern, isExact, seen, routes);
            }
        }

        private void AddAssetPrefixRoute(
            string? assetPrefix,
            HashSet<(string ZoneName, string Pattern)> seen,
            List<MicrofrontendRouteDefinition> routes
        )
        {
            if (string.IsNullOrWhiteSpace(assetPrefix))
            {
                return;
            }

            var trimmedPrefix = assetPrefix.Trim().Trim('/');

            if (string.IsNullOrEmpty(trimmedPrefix))
            {
                return;
            }

            appName.TryAddRoute($"/{trimmedPrefix}/{CatchAllSuffix}", false, seen, routes);
        }

        private void TryAddRoute(
            string pattern,
            bool isExact,
            HashSet<(string ZoneName, string Pattern)> seen,
            List<MicrofrontendRouteDefinition> routes
        )
        {
            if (seen.Add((appName, pattern)))
            {
                routes.Add(new(appName, pattern, isExact));
            }
        }
    }

    /// <summary>
    ///     Translates a Vercel <c>path-to-regexp</c> route expression into an ASP.NET Core / YARP route pattern.
    /// </summary>
    /// <param name="vercelPath">A path expression from <c>microfrontends.json</c>.</param>
    /// <returns>A tuple containing the YARP route template and whether it is an exact (non-catch-all) route.</returns>
    private static (string Pattern, bool IsExact) TranslatePathPattern(string vercelPath)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(vercelPath);

        var normalized = vercelPath.Trim();

        if (!normalized.StartsWith('/'))
        {
            normalized = $"/{normalized}";
        }

        var catchAllMatch = TrailingCatchAllRegex().Match(normalized);

        if (catchAllMatch.Success)
        {
            var prefix = normalized[..catchAllMatch.Index].TrimEnd('/');
            var translatedPrefix = TranslateInlineParameters(prefix);
            var pattern = string.IsNullOrEmpty(translatedPrefix)
                ? $"/{CatchAllSuffix}"
                : $"{translatedPrefix}/{CatchAllSuffix}";

            return (pattern, false);
        }

        var exactPath = normalized.Length > 1 ? normalized.TrimEnd('/') : normalized;

        return (TranslateInlineParameters(exactPath), true);
    }

    /// <summary>
    ///     Determines whether a route pattern's leading path segment collides with a registered backend service prefix.
    /// </summary>
    /// <param name="pattern">The YARP route pattern (e.g., <c>/basket</c>).</param>
    /// <param name="serviceNames">Registered backend service prefixes (case-insensitive).</param>
    /// <returns><see langword="true" /> when the first literal path segment matches a registered service name.</returns>
    public static bool CollidesWithService(string pattern, IReadOnlySet<string> serviceNames)
    {
        if (string.IsNullOrWhiteSpace(pattern) || serviceNames.Count == 0)
        {
            return false;
        }

        var trimmed = pattern.TrimStart('/');

        if (string.IsNullOrEmpty(trimmed))
        {
            return false;
        }

        var slashIndex = trimmed.IndexOf('/');
        var firstSegment = slashIndex >= 0 ? trimmed[..slashIndex] : trimmed;

        return !firstSegment.StartsWith('{') && serviceNames.Contains(firstSegment);
    }

    private static string TranslateInlineParameters(string path)
    {
        return string.IsNullOrEmpty(path) ? path : InlineParameterRegex().Replace(path, "/{$1}");
    }

    [GeneratedRegex(@"/(?::[A-Za-z_][A-Za-z0-9_]*[\*\+]|\*|\(\.\*\))/?$")]
    private static partial Regex TrailingCatchAllRegex();

    [GeneratedRegex(@"/:([A-Za-z_][A-Za-z0-9_]*)")]
    private static partial Regex InlineParameterRegex();
}

internal sealed record MicrofrontendsConfig(
    [property: JsonPropertyName("applications")]
        IReadOnlyDictionary<string, MicrofrontendApplicationConfig?>? Applications
);

internal sealed record MicrofrontendApplicationConfig(
    [property: JsonPropertyName("assetPrefix")] string? AssetPrefix,
    [property: JsonPropertyName("routing")] IReadOnlyList<MicrofrontendRoutingGroup>? Routing
);

internal sealed record MicrofrontendRoutingGroup(
    [property: JsonPropertyName("paths")] IReadOnlyList<string>? Paths
);

[JsonSerializable(typeof(MicrofrontendsConfig))]
[JsonSourceGenerationOptions(
    PropertyNameCaseInsensitive = true,
    ReadCommentHandling = JsonCommentHandling.Skip,
    AllowTrailingCommas = true
)]
internal sealed partial class MicrofrontendsConfigSerializationContext : JsonSerializerContext;
