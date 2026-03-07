using BookWorm.Finance.Saga;
using BookWorm.Finance.Saga.Observers;
using MassTransit;

namespace BookWorm.Finance.UnitTests;

public sealed class OrderStateObserverTests
{
    [Test]
    public async Task GivenStateTransition_WhenStateChanged_ThenShouldComplete()
    {
        // Arrange
        var observer = new OrderStateObserver();
        var saga = new OrderState { OrderId = Guid.CreateVersion7() };
        var context = new Mock<BehaviorContext<OrderState>>();
        context.Setup(c => c.Saga).Returns(saga);

        var currentState = new Mock<State>();
        currentState.Setup(s => s.Name).Returns("Placed");
        var previousState = new Mock<State>();
        previousState.Setup(s => s.Name).Returns("Initial");

        // Act & Assert — should complete without throwing
        await observer.StateChanged(context.Object, currentState.Object, previousState.Object);
    }

    [Test]
    public async Task GivenMultipleTransitions_WhenStateChanged_ThenShouldCompleteEachTime()
    {
        // Arrange
        var observer = new OrderStateObserver();
        var saga = new OrderState { OrderId = Guid.CreateVersion7() };
        var context = new Mock<BehaviorContext<OrderState>>();
        context.Setup(c => c.Saga).Returns(saga);

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
            var current = new Mock<State>();
            current.Setup(s => s.Name).Returns(to);
            var previous = new Mock<State>();
            previous.Setup(s => s.Name).Returns(from);

            await observer.StateChanged(context.Object, current.Object, previous.Object);
        }
    }
}
