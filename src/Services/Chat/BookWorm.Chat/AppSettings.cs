using System.Diagnostics.CodeAnalysis;

namespace BookWorm.Chat;

[ExcludeFromCodeCoverage]
public sealed class AppSettings
{
    public TimeSpan StreamTimeout { get; set; }
    public int MaxMessages { get; set; }
}
