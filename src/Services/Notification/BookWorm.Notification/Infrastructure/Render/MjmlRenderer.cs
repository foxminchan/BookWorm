using System.Reflection;
using BookWorm.Notification.Infrastructure.Attributes;

namespace BookWorm.Notification.Infrastructure.Render;

public sealed class MjmlRenderer : IRenderer
{
    public string Render<T>(T model, string template)
        where T : notnull
    {
        var mjml = File.ReadAllText(template);
        var properties = typeof(T).GetProperties(BindingFlags.Public | BindingFlags.Instance);

        properties
            .AsValueEnumerable()
            .ToList()
            .ForEach(property =>
            {
                var value = GetFormattedValue(property, model);
                mjml = mjml.Replace($"{{{{{property.Name}}}}}", value);
            });

        mjml = mjml.Replace("{{Year}}", DateTime.Now.Year.ToString());

        return mjml;
    }

    private static string GetFormattedValue(PropertyInfo property, object model)
    {
        var value = property.GetValue(model);
        if (value is null)
        {
            return string.Empty;
        }

        var formatAttribute = property.GetCustomAttribute<FormatAttribute>();
        if (formatAttribute is null)
        {
            return value.ToString() ?? string.Empty;
        }

        if (value is IFormattable formattable)
        {
            return formattable.ToString(formatAttribute.Format, formatAttribute.FormatProvider);
        }

        return value.ToString() ?? string.Empty;
    }
}
