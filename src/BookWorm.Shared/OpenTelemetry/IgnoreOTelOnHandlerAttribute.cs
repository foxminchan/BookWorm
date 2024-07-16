namespace BookWorm.Shared.OpenTelemetry;

[AttributeUsage(AttributeTargets.Class)]
public sealed class IgnoreOTelOnHandlerAttribute : Attribute;
