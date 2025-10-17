using BookWorm.Finance.Saga;

namespace BookWorm.Finance.UnitTests.Helpers;

public static class OrderStateMachineTestHelper
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

    public static void AssertOrderState(OrderState instance, TestOrderData testData)
    {
        instance.OrderId.ShouldBe(testData.OrderId);
        instance.BasketId.ShouldBe(testData.BasketId);
        instance.Email.ShouldBe(testData.Email);
        instance.TotalMoney.ShouldBe(testData.TotalMoney);
        instance.Version.ShouldBeGreaterThanOrEqualTo(0);
        instance.RowVersion.ShouldBeGreaterThanOrEqualTo(0u);
        instance.OrderPlacedDate.ShouldNotBe(DateTime.MinValue);
    }

    public record TestOrderData(
        Guid OrderId,
        Guid BasketId,
        string? FullName = DefaultTestFullName,
        string? Email = DefaultTestEmail,
        decimal TotalMoney = DefaultTotalMoney
    );
}
