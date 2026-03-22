namespace BookWorm.Chassis.CQRS;

/// <summary>
/// Marks a command as requiring ACID transaction wrapping.
/// When a command is decorated with this attribute, the <c>TransactionBehavior</c> pipeline
/// will automatically begin a database transaction before the handler executes
/// and commit it upon success (or roll back on failure).
/// </summary>
[AttributeUsage(AttributeTargets.Class, Inherited = false)]
public sealed class TransactionalAttribute : Attribute;
