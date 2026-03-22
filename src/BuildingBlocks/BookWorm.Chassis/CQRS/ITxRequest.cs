namespace BookWorm.Chassis.CQRS;

/// <summary>
/// Marker interface for commands that require ACID transaction wrapping.
/// When a command implements this interface, the <c>TransactionBehavior</c> pipeline
/// will automatically begin a database transaction before the handler executes
/// and commit it upon success (or roll back on failure).
/// </summary>
public interface ITxRequest;
