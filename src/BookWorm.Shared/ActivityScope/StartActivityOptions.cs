using System.Diagnostics;

namespace BookWorm.Shared.ActivityScope;

public sealed record StartActivityOptions
{
    public Dictionary<string, object?> Tags { get; set; } = [];

    public string? ParentId { get; set; }

    public ActivityContext? Parent { get; set; }

    public readonly ActivityKind Kind = ActivityKind.Internal;
}
