namespace BookWorm.SharedKernel.OpenTelemetry;

[AttributeUsage(AttributeTargets.Class)]
public sealed class IgnoreOTelOnHandlerAttribute : Attribute;
