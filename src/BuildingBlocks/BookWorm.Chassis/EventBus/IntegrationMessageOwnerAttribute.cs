namespace BookWorm.Chassis.EventBus;

[AttributeUsage(AttributeTargets.Class, Inherited = false)]
public sealed class IntegrationMessageOwnerAttribute : Attribute
{
    public string ServiceName { get; set; } = string.Empty;
    public string TopicName { get; set; } = string.Empty;
    public bool AppendPrefix { get; set; } = true;
}
