using BookWorm.Common;
using BookWorm.Contracts;
using BookWorm.Finance.Saga;
using Microsoft.Extensions.Logging;
using Wolverine;

namespace BookWorm.Finance.ContractTests.Saga;

[Category("PactProvider")]
public sealed class OrderStateMachineProviderTests
{
    private const string FullName = "John Doe";
    private const string Email = "john.doe@example.com";
    private const decimal TotalMoney = 99.99m;

    [Test]
    public void GivenCheckout_WhenStartingSaga_ThenShouldHonorBasketPlaceOrderContract()
    {
        var messages = StartSaga().Messages;

        PactTestHelper.VerifyProviderMessage(
            "Basket",
            "Finance",
            messages.OfType<PlaceOrderCommand>().Single()
        );
    }

    [Test]
    public void GivenCheckout_WhenStartingSaga_ThenShouldHonorNotificationPlaceOrderContract()
    {
        var messages = StartSaga().Messages;

        PactTestHelper.VerifyProviderMessage(
            "Notification",
            "Finance",
            messages.OfType<PlaceOrderCommand>().Single()
        );
    }

    [Test]
    public void GivenBasketDeletionComplete_WhenHandling_ThenShouldHonorOrderingContract()
    {
        var (saga, _) = StartSaga();
        var command = saga.Handle(new(Guid.CreateVersion7(), Guid.CreateVersion7(), TotalMoney));

        PactTestHelper.VerifyProviderMessage("Ordering", "Finance", command);
    }

    [Test]
    public void GivenBasketDeletionFailed_WhenHandling_ThenShouldHonorOrderingContract()
    {
        var (saga, _) = StartSaga();
        var messages = saga.Handle(
            new BasketDeletedFailedIntegrationEvent(
                Guid.CreateVersion7(),
                Guid.CreateVersion7(),
                Email,
                TotalMoney
            ),
            Mock.Of<ILogger<OrderSaga>>()
        );

        PactTestHelper.VerifyProviderMessage(
            "Ordering",
            "Finance",
            messages.OfType<DeleteBasketFailedCommand>().Single()
        );
    }

    [Test]
    public void GivenOrderCompleted_WhenHandling_ThenShouldHonorNotificationContract()
    {
        var (saga, _) = StartSaga();
        var messages = saga.Handle(
            new OrderStatusChangedToCompleteIntegrationEvent(
                Guid.CreateVersion7(),
                Guid.CreateVersion7(),
                FullName,
                Email,
                TotalMoney
            ),
            Mock.Of<ILogger<OrderSaga>>()
        );

        PactTestHelper.VerifyProviderMessage(
            "Notification",
            "Finance",
            messages.OfType<CompleteOrderCommand>().Single()
        );
    }

    [Test]
    public void GivenOrderCancelled_WhenHandling_ThenShouldHonorNotificationContract()
    {
        var (saga, _) = StartSaga();
        var messages = saga.Handle(
            new OrderStatusChangedToCancelIntegrationEvent(
                Guid.CreateVersion7(),
                Guid.CreateVersion7(),
                FullName,
                Email,
                TotalMoney
            ),
            Mock.Of<ILogger<OrderSaga>>()
        );

        PactTestHelper.VerifyProviderMessage(
            "Notification",
            "Finance",
            messages.OfType<CancelOrderCommand>().Single()
        );
    }

    private static (OrderSaga Saga, OutgoingMessages Messages) StartSaga()
    {
        var checkout = new UserCheckedOutIntegrationEvent(
            Guid.CreateVersion7(),
            Guid.CreateVersion7(),
            FullName,
            Email,
            TotalMoney
        );
        var settings = new OrderStateMachineSettings
        {
            MaxAttempts = 3,
            MaxRetryTimeout = TimeSpan.FromMinutes(30),
        };

        return OrderSaga.Start(checkout, settings, Mock.Of<ILogger<OrderSaga>>());
    }
}
