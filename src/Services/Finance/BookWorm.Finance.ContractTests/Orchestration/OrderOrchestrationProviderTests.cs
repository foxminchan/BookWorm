using BookWorm.Contracts;

namespace BookWorm.Finance.ContractTests.Orchestration;

[Category("PactProvider")]
public sealed class OrderOrchestrationProviderTests
{
    private const string FullName = "John Doe";
    private const string Email = "john.doe@example.com";
    private const decimal TotalMoney = 99.99m;

    [Test]
    public void GivenCheckout_WhenStartingOrchestration_ThenShouldHonorBasketPlaceOrderContract()
    {
        var command = new PlaceOrderCommand(
            Guid.CreateVersion7(),
            FullName,
            Email,
            Guid.CreateVersion7(),
            TotalMoney
        );

        PactTests.Helper.VerifyProviderMessage("Basket", "Finance", command);
    }

    [Test]
    public void GivenCheckout_WhenStartingOrchestration_ThenShouldHonorNotificationPlaceOrderContract()
    {
        var command = new PlaceOrderCommand(
            Guid.CreateVersion7(),
            FullName,
            Email,
            Guid.CreateVersion7(),
            TotalMoney
        );

        PactTests.Helper.VerifyProviderMessage("Notification", "Finance", command);
    }

    [Test]
    public void GivenBasketDeletionComplete_WhenHandling_ThenShouldHonorOrderingContract()
    {
        var command = new DeleteBasketCompleteCommand(Guid.CreateVersion7(), TotalMoney);

        PactTests.Helper.VerifyProviderMessage("Ordering", "Finance", command);
    }

    [Test]
    public void GivenBasketDeletionFailed_WhenHandling_ThenShouldHonorOrderingContract()
    {
        var command = new DeleteBasketFailedCommand(
            Guid.CreateVersion7(),
            Email,
            Guid.CreateVersion7(),
            TotalMoney
        );

        PactTests.Helper.VerifyProviderMessage("Ordering", "Finance", command);
    }

    [Test]
    public void GivenOrderCompleted_WhenHandling_ThenShouldHonorNotificationContract()
    {
        var command = new CompleteOrderCommand(Guid.CreateVersion7(), FullName, Email, TotalMoney);

        PactTests.Helper.VerifyProviderMessage("Notification", "Finance", command);
    }

    [Test]
    public void GivenOrderCancelled_WhenHandling_ThenShouldHonorNotificationContract()
    {
        var command = new CancelOrderCommand(Guid.CreateVersion7(), FullName, Email, TotalMoney);

        PactTests.Helper.VerifyProviderMessage("Notification", "Finance", command);
    }
}
