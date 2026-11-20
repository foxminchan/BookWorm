namespace BookWorm.Chassis.AI.Decisions.Abstractions;

public interface IDecisionClient : IDisposable
{
    Task<DecisionResponse> GetDecisionAsync(
        DecisionRequest request,
        DecisionOptions? options = null,
        CancellationToken cancellationToken = default
    );

    object? GetService(Type serviceType, object? serviceKey = null);
}
