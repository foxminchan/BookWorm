using System.Reflection;
using System.Text.RegularExpressions;
using BookWorm.Notification.Infrastructure.Attributes;
using BookWorm.SharedKernel.Helpers;
using Mjml.Net;

namespace BookWorm.Notification.Infrastructure.Render;

internal sealed partial class MjmlTemplateRenderer : IRenderer
{
    private static readonly Assembly Assembly = typeof(MjmlTemplateRenderer).Assembly;

    private static readonly MjmlRenderer _mjmlCompiler = new();

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

        // Inject content into the layout
        var combinedMjml = layoutMjml.Replace("{{Content}}", contentMjml);

        // Substitute model properties and built-in variables
        combinedMjml = SubstituteModelProperties(combinedMjml, model);
        combinedMjml = combinedMjml.Replace("{{Year}}", DateTimeHelper.UtcNow().Year.ToString());

        // Remove any remaining unreplaced variables (e.g., {{PreviewText}} when not provided)
        combinedMjml = VariablePattern().Replace(combinedMjml, string.Empty);

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
        var resourceName =
            $"{Assembly.GetName().Name}.Templates.{templateName.Replace('/', '.')}.mjml";

        await using var stream =
            Assembly.GetManifestResourceStream(resourceName)
            ?? throw new FileNotFoundException(
                $"Embedded MJML template '{templateName}' not found as resource '{resourceName}'"
            );

        using var reader = new StreamReader(stream);
        return await reader.ReadToEndAsync(cancellationToken);
    }

    private static string SubstituteModelProperties<T>(string mjml, T model)
        where T : notnull
    {
        var properties = typeof(T).GetProperties(BindingFlags.Public | BindingFlags.Instance);

        return VariablePattern()
            .Replace(
                mjml,
                match =>
                {
                    var name = match.Groups[1].Value;
                    var property = properties.FirstOrDefault(p => p.Name == name);
                    if (property is null)
                    {
                        return match.Value;
                    }

                    return GetFormattedValue(property, model);
                }
            );
    }

    private static string GetFormattedValue(PropertyInfo property, object model)
    {
        var value = property.GetValue(model);
        if (value is null)
        {
            return string.Empty;
        }

        var formatAttribute = property.GetCustomAttribute<FormatAttribute>();
        if (formatAttribute is not null && value is IFormattable formattable)
        {
            return formattable.ToString(formatAttribute.Format, formatAttribute.FormatProvider);
        }

        return value.ToString() ?? string.Empty;
    }

    [GeneratedRegex(@"\{\{(\w+)\}\}")]
    private static partial Regex VariablePattern();
}
