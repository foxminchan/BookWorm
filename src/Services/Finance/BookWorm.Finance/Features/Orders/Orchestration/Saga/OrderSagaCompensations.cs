namespace BookWorm.Finance.Features.Orders.Orchestration.Saga;

/// <summary>
///     Tracks and executes Saga compensating actions in reverse (LIFO) order when an order orchestration fails.
/// </summary>
internal sealed class OrderSagaCompensations(TaskOrchestrationContext context)
{
    private readonly List<(string ActivityName, object Command)> _compensations = [];

    /// <summary>
    ///     Registers a compensating activity to run if a subsequent step in the Saga fails.
    ///     Should be called before executing the corresponding forward activity.
    /// </summary>
    public void AddCompensation<T>(string activityName, T command)
        where T : notnull
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(activityName);
        ArgumentNullException.ThrowIfNull(command);

        _compensations.Add((activityName, command));
    }

    /// <summary>
    ///     Clears all registered compensations once the Saga reaches a normal terminal outcome.
    /// </summary>
    public void Clear()
    {
        _compensations.Clear();
    }

    /// <summary>
    ///     Executes all registered compensating activities in LIFO order (or in parallel),
    ///     isolating individual compensation failures so remaining compensations still run.
    /// </summary>
    public async Task CompensateAsync(bool inParallel = false)
    {
        if (_compensations.Count == 0)
        {
            return;
        }

        if (inParallel)
        {
            var tasks = _compensations.Select(compensation =>
                ExecuteCompensationWithErrorHandlingAsync(
                    compensation.ActivityName,
                    compensation.Command
                )
            );
            await Task.WhenAll(tasks);
        }
        else
        {
            for (var i = _compensations.Count - 1; i >= 0; i--)
            {
                var (activityName, command) = _compensations[i];
                await ExecuteCompensationWithErrorHandlingAsync(activityName, command);
            }
        }

        _compensations.Clear();
    }

    private async Task ExecuteCompensationWithErrorHandlingAsync(
        string activityName,
        object command
    )
    {
        var logger = context.CreateReplaySafeLogger<OrderSagaCompensations>();

        try
        {
            logger.LogInformation(
                "[OrderOrchestrator] Executing Saga compensation activity {ActivityName}",
                activityName
            );
            await context.PublishActivityAsync(activityName, command);
        }
        catch (Exception ex)
        {
            logger.LogError(
                ex,
                "[OrderOrchestrator] Saga compensation activity {ActivityName} failed; continuing remaining compensations",
                activityName
            );
        }
    }
}
