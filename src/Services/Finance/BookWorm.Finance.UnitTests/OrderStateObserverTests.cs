using BookWorm.Finance.Saga.Observers;

namespace BookWorm.Finance.UnitTests;

public sealed class OrderStateObserverTests
{
    [Test]
    public void GivenStateTransition_WhenStateChanged_ThenShouldComplete()
    {
        // Arrange
        var observer = new OrderSagaStateObserver();
        var orderId = Guid.CreateVersion7();

        // Act & Assert — should complete without throwing
        observer.RecordTransition(orderId, "Initial", "Placed");
    }

    [Test]
    public void GivenMultipleTransitions_WhenStateChanged_ThenShouldCompleteEachTime()
    {
        // Arrange
        var observer = new OrderSagaStateObserver();
        var orderId = Guid.CreateVersion7();

        var transitions = new (string From, string To)[]
        {
            ("Initial", "Placed"),
            ("Placed", "Completed"),
            ("Placed", "Cancelled"),
            ("Placed", "Failed"),
        };

        // Act & Assert
        foreach (var (from, to) in transitions)
        {
            observer.RecordTransition(orderId, from, to);
        }
    }
}
