namespace BookWorm.Chassis.EventBus;

[AttributeUsage(AttributeTargets.Class, Inherited = false)]
public sealed class IntegrationMessageOwnerAttribute : Attribute
{
    /// <summary>
    ///     Gets or sets the service segment of the Kafka topic.
    /// </summary>
    public string ServiceName { get; set; } = string.Empty;

    /// <summary>
    ///     Gets or sets the explicit Kafka topic name segment.
    /// </summary>
    public string TopicName { get; set; } = string.Empty;

    /// <summary>
    ///     Gets or sets whether the project and environment prefix is added to the topic.
    /// </summary>
    public bool AppendPrefix { get; set; } = true;
}
