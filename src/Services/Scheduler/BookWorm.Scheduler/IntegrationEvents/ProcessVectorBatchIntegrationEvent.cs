namespace BookWorm.Contracts;

[IntegrationMessageOwner(
    ServiceName = Services.Scheduler,
    TopicName = "process-vector-batch",
    AppendPrefix = false
)]
[MessageIdentity("BookWorm.Contracts.ProcessVectorBatchIntegrationEvent")]
public sealed record ProcessVectorBatchIntegrationEvent : IntegrationEvent;
