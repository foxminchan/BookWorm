namespace BookWorm.Finance.Features.Orders.Orchestration.Models;

public sealed record OrderOrchestrationInput(
    Guid OrderId,
    Guid BasketId,
    string? FullName,
    string? Email,
    decimal TotalMoney,
    int MaxAttempts,
    TimeSpan MaxRetryTimeout
);
