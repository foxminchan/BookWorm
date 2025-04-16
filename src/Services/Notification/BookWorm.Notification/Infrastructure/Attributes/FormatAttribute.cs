using System.Globalization;

namespace BookWorm.Notification.Infrastructure.Attributes;

[AttributeUsage(AttributeTargets.Property)]
public sealed class FormatAttribute(string format, string? cultureName = null) : Attribute
{
    public string Format { get; } = format;
    public IFormatProvider? FormatProvider { get; } =
        cultureName is not null ? CultureInfo.GetCultureInfo(cultureName) : null;
}
