using System.Globalization;

namespace BookWorm.Notification.Domain.Attributes;

[ExcludeFromCodeCoverage]
[AttributeUsage(AttributeTargets.Property)]
internal sealed class FormatAttribute(
    [StringSyntax(StringSyntaxAttribute.CompositeFormat)] string format
) : Attribute
{
    public string Format { get; } = format;

    public IFormatProvider? FormatProvider { get; } = CultureInfo.InvariantCulture;
}
