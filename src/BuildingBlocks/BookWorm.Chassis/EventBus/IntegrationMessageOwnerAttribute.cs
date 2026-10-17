namespace BookWorm.Chassis.EventBus;

[AttributeUsage(AttributeTargets.Class, Inherited = false)]
public sealed class IntegrationMessageOwnerAttribute(string serviceName) : Attribute
{
    public string ServiceName { get; } = serviceName;
}
