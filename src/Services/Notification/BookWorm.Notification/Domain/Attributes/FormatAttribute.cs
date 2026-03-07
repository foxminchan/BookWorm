using System.Globalization;

namespace BookWorm.Notification.Domain.Attributes;

/// <summary>
///     Specifies a composite format string and optional culture for formatting
///     property values when rendering email templates.
/// </summary>
[AttributeUsage(AttributeTargets.Property)]
public sealed class FormatAttribute(
    [StringSyntax(StringSyntaxAttribute.CompositeFormat)] string format,
    string? cultureName = null
) : Attribute
{
    public string Format { get; } = format;

    public IFormatProvider? FormatProvider { get; } =
        cultureName is not null ? CultureInfo.GetCultureInfo(cultureName) : null;
}
