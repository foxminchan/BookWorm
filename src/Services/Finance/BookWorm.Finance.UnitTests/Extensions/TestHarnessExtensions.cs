using BookWorm.Contracts;
using BookWorm.Finance.Saga;
using BookWorm.Finance.UnitTests.Helpers;
using MassTransit;
using MassTransit.Testing;

namespace BookWorm.Finance.UnitTests.Extensions;

internal static class TestHarnessExtensions
{
    public static async Task InitializeSagaToPlacedState(
        this (
            ITestHarness harness,
            ISagaStateMachineTestHarness<OrderStateMachine, OrderState> sagaHarness
        ) context,
        OrderStateMachineTestHelper.TestOrderData? testData = null
    )
    {
        testData ??= OrderStateMachineTestHelper.CreateTestOrderData();

        var checkoutEvent = new UserCheckedOutIntegrationEvent(
            testData.OrderId,
            testData.BasketId,
            testData.FullName,
            testData.Email,
            testData.TotalMoney
        );

        await context.harness.Bus.Publish(checkoutEvent);
        await context.sagaHarness.Created.Any(x => x.CorrelationId == testData.OrderId);
    }

    extension(ISagaStateMachineTestHarness<OrderStateMachine, OrderState> sagaHarness)
    {
        public OrderState AssertSagaInState(Guid orderId, State state)
        {
            var instance = sagaHarness.Created.ContainsInState(
                orderId,
                sagaHarness.StateMachine,
                state
            );
            instance.ShouldNotBeNull();
            instance.CurrentState.ShouldBe(state.Name);
            return instance;
        }

        public void AssertSagaFinalized(Guid orderId)
        {
            var states = new[]
            {
                sagaHarness.StateMachine.Placed,
                sagaHarness.StateMachine.Completed,
                sagaHarness.StateMachine.Cancelled,
                sagaHarness.StateMachine.Failed,
            };

            var sagaInAnyState = states.Any(state =>
                sagaHarness.Created.ContainsInState(orderId, sagaHarness.StateMachine, state)
                    is not null
            );

            sagaInAnyState.ShouldBeFalse("Saga should be finalized and not in any active state");
        }
    }

    extension(ITestHarness harness)
    {
        public async Task<TCommand> AssertCommandPublished<TCommand>()
            where TCommand : class
        {
            (await harness.WaitForCommandPublished<TCommand>()).ShouldBeTrue();
            var publishedMessage = harness.Published.Select<TCommand>().First();
            return publishedMessage.Context.Message;
        }

        public async Task AssertCommandNotPublished<TCommand>(TimeSpan? timeout = null)
            where TCommand : class
        {
            timeout ??= TimeSpan.FromSeconds(5);
            (await harness.WaitForCommandPublished<TCommand>(timeout)).ShouldBeFalse();
        }

        private async Task<bool> WaitForCommandPublished<TCommand>(TimeSpan? timeout = null)
            where TCommand : class
        {
            timeout ??= TimeSpan.FromSeconds(30);
            using var cts = new CancellationTokenSource(timeout.Value);

            try
            {
                return await harness.Published.Any<TCommand>(cts.Token);
            }
            catch (OperationCanceledException)
            {
                return false;
            }
        }

        public async Task AssertEventConsumed<TEvent>(TimeSpan? timeout = null)
            where TEvent : class
        {
            timeout ??= TimeSpan.FromSeconds(60);
            using var cts = new CancellationTokenSource(timeout.Value);
            (await harness.Consumed.Any<TEvent>(cts.Token)).ShouldBeTrue();
        }
    }
}
