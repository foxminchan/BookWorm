using System.Collections.Concurrent;
using System.Collections.Frozen;
using System.Globalization;
using System.Reflection;
using System.Text.RegularExpressions;
using BookWorm.Notification.Domain.Attributes;
using Mjml.Net;

namespace BookWorm.Notification.Infrastructure.Render;

internal sealed partial class MjmlTemplateRenderer : IRenderer
{
    private static readonly Assembly _assembly = typeof(MjmlTemplateRenderer).Assembly;

    private static readonly MjmlRenderer _mjmlCompiler = new();

    private static readonly ConcurrentDictionary<string, string> _templateCache = new(
        StringComparer.Ordinal
    );

    public async Task<string> RenderAsync<T>(
        T model,
        string templateName,
        CancellationToken cancellationToken = default
    )
        where T : notnull
    {
        // Load content template and layout
        var contentMjml = await LoadTemplateAsync(templateName, cancellationToken);
        var layoutMjml = await LoadTemplateAsync("_Layout", cancellationToken);

        // Inject content into the layout and substitute variables in a single pass
        var combinedMjml = layoutMjml.Replace("{{Content}}", contentMjml, StringComparison.Ordinal);
        combinedMjml = SubstituteVariables(combinedMjml, model);

        var result = await _mjmlCompiler.RenderAsync(combinedMjml, ct: cancellationToken);

        if (result.Errors.Count > 0)
        {
            var errors = string.Join(", ", result.Errors.Select(e => e.Error));
            throw new InvalidOperationException(
                $"MJML compilation failed for template '{templateName}': {errors}"
            );
        }

        return result.Html;
    }

    private static async Task<string> LoadTemplateAsync(
        string templateName,
        CancellationToken cancellationToken
    )
    {
        if (_templateCache.TryGetValue(templateName, out var cached))
        {
            return cached;
        }

        var resourceName =
            $"{_assembly.GetName().Name}.Templates.{templateName.Replace('/', '.')}.mjml";

        await using var stream = _assembly.GetManifestResourceStream(resourceName)
            ?? throw new FileNotFoundException(
                $"Embedded MJML template '{templateName}' not found as resource '{resourceName}'"
            );

        using var reader = new StreamReader(stream);
        var template = await reader.ReadToEndAsync(cancellationToken);
        _templateCache.TryAdd(templateName, template);

        return template;
    }

    private static string SubstituteVariables<T>(string mjml, T model)
        where T : notnull
    {
        var properties = ModelPropertyCache<T>.Properties;
        string? year = null;

        return VariablePattern()
            .Replace(mjml, match =>
            {
                var key = match.Groups[1].Value;

                if (properties.TryGetValue(key, out var cachedProperty))
                {
                    return GetFormattedValue(cachedProperty, model);
                }

                if (string.Equals(key, "Year", StringComparison.Ordinal))
                {
                    return year ??= DateTime.UtcNow.Year.ToString(CultureInfo.InvariantCulture);
                }

                // Remove any remaining unreplaced variables (e.g., {{PreviewText}} when not provided)
                return string.Empty;
            });
    }

    private static string GetFormattedValue(CachedProperty cachedProperty, object model)
    {
        var value = cachedProperty.Property.GetValue(model);
        if (value is null)
        {
            return string.Empty;
        }

        if (cachedProperty.Format is { } formatAttribute && value is IFormattable formattable)
        {
            return formattable.ToString(formatAttribute.Format, formatAttribute.FormatProvider);
        }

        return value.ToString() ?? string.Empty;
    }

    [GeneratedRegex(@"\{\{(\w+)\}\}")]
    private static partial Regex VariablePattern();

    private sealed record CachedProperty(PropertyInfo Property, FormatAttribute? Format);

    private static class ModelPropertyCache<T>
    {
        public static readonly FrozenDictionary<string, CachedProperty> Properties = typeof(T)
            .GetProperties(BindingFlags.Public | BindingFlags.Instance)
            .ToFrozenDictionary(
                p => p.Name,
                p => new CachedProperty(p, p.GetCustomAttribute<FormatAttribute>()),
                StringComparer.Ordinal
            );
    }
}
