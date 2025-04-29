namespace BookWorm.Finance.Saga;

public sealed class OrderState : SagaStateMachineInstance, ISagaVersion
{
    public Guid OrderId { get; set; }
    public Guid BasketId { get; set; }
    public string? FullName { get; set; }
    public string? Email { get; set; }
    public string CurrentState { get; set; } = null!;
    public decimal? TotalMoney { get; set; }
    public DateTime? OrderPlacedDate { get; set; }
    public int Version { get; set; }
    public Guid CorrelationId { get; set; }
}
