using System.Data;

namespace BookWorm.Chassis.CQRS;

/// <summary>
/// Marks a command as requiring ACID transaction wrapping.
/// When a command is decorated with this attribute, the <c>TransactionBehavior</c> pipeline
/// will automatically begin a database transaction before the handler executes
/// and commit it upon success (or roll back on failure).
/// </summary>
/// <param name="isolationLevel">
/// The isolation level for the transaction.
/// Defaults to <see cref="IsolationLevel.ReadCommitted"/>, which prevents dirty reads
/// and is appropriate for most OLTP write commands.
/// </param>
[AttributeUsage(AttributeTargets.Class, Inherited = false)]
public sealed class TransactionalAttribute(
    IsolationLevel isolationLevel = IsolationLevel.ReadCommitted
) : Attribute
{
    /// <summary>Gets the isolation level used when beginning the transaction.</summary>
    public IsolationLevel IsolationLevel { get; } = isolationLevel;
}
