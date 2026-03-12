using BookWorm.Finance.Saga;
using Microsoft.Extensions.Options;
using Wolverine;

namespace BookWorm.Finance.UnitTests.Helpers;

internal static class OrderSagaTestHelper
{
    public const string DefaultTestEmail = "example@email.com";
    public const string DefaultTestFullName = "John Doe";
    private const decimal DefaultTotalMoney = 100.0m;

    public static TestOrderData CreateTestOrderData(
        Guid? orderId = null,
        Guid? basketId = null,
        string? fullName = DefaultTestFullName,
        string? email = DefaultTestEmail,
        decimal totalMoney = DefaultTotalMoney
    )
    {
        return new(
            orderId ?? Guid.CreateVersion7(),
            basketId ?? Guid.CreateVersion7(),
            fullName,
            email,
            totalMoney
        );
    }

    public static void AssertSagaState(OrderSaga saga, TestOrderData testData)
    {
        saga.Id.ShouldBe(testData.OrderId);
        saga.BasketId.ShouldBe(testData.BasketId);
        saga.Email.ShouldBe(testData.Email);
        saga.TotalMoney.ShouldBe(testData.TotalMoney);
        saga.OrderPlacedDate.ShouldNotBeNull();
    }

    public static IOptions<OrderStateMachineSettings> CreateSettings(
        int maxAttempts = 3,
        TimeSpan? maxRetryTimeout = null
    )
    {
        return Options.Create(
            new OrderStateMachineSettings
            {
                MaxAttempts = maxAttempts,
                MaxRetryTimeout = maxRetryTimeout ?? TimeSpan.FromMinutes(30),
            }
        );
    }

    public static Mock<IMessageContext> CreateMessageContextMock(List<object>? published = null)
    {
        published ??= [];
        var mock = new Mock<IMessageContext>();
        mock.Setup(x => x.PublishAsync(It.IsAny<object>(), It.IsAny<DeliveryOptions?>()))
            .Callback<object, DeliveryOptions?>((msg, _) => published.Add(msg))
            .Returns(ValueTask.CompletedTask);
        mock.Setup(x => x.SendAsync(It.IsAny<object>(), It.IsAny<DeliveryOptions?>()))
            .Callback<object, DeliveryOptions?>((msg, _) => published.Add(msg))
            .Returns(ValueTask.CompletedTask);
        return mock;
    }

    public record TestOrderData(
        Guid OrderId,
        Guid BasketId,
        string? FullName = DefaultTestFullName,
        string? Email = DefaultTestEmail,
        decimal TotalMoney = DefaultTotalMoney
    );
}
