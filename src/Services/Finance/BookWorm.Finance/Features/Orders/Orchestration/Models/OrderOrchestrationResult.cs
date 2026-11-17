namespace BookWorm.Finance.Features.Orders.Orchestration.Models;

public sealed record OrderOrchestrationResult(
    Guid OrderId,
    Guid BasketId,
    string? FullName,
    string? Email,
    decimal TotalMoney,
    OrderOrchestrationStatus FinalStatus,
    int TimeoutRetryCount,
    DateTime OrderPlacedDate,
    string? FailureReason = null
);
