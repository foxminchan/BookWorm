using Microsoft.Extensions.Compliance.Redaction;

namespace BookWorm.Chassis.Logging;

internal sealed class AsteriskRedactor : Redactor
{
    public override int Redact(ReadOnlySpan<char> source, Span<char> destination)
    {
        destination[..source.Length].Fill('*');
        return source.Length;
    }

    public override int GetRedactedLength(ReadOnlySpan<char> input)
    {
        return input.Length;
    }
}
