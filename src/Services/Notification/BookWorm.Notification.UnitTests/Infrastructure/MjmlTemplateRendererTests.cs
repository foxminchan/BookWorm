using BookWorm.Notification.Domain.Models;
using BookWorm.Notification.Infrastructure.Render;

namespace BookWorm.Notification.UnitTests.Infrastructure;

public sealed class MjmlTemplateRendererTests
{
    private readonly MjmlTemplateRenderer _renderer = new();

    [Test]
    public async Task GivenNewOrder_WhenRenderingOrderEmail_ThenShouldMatchSnapshot()
    {
        // Arrange
        var order = new Order(
            new("01961f3a-0000-7000-8000-000000000001"),
            "John Doe",
            99.99m,
            Status.New
        )
        {
            CreatedAt = new(2025, 6, 15),
        };

        // Act
        var html = await _renderer.RenderAsync(order, "Orders/OrderEmail", CancellationToken.None);

        // Assert
        await Verify(html, "html").ScrubLinesContaining("© ");
    }

    [Test]
    public async Task GivenCompletedOrder_WhenRenderingOrderEmail_ThenShouldMatchSnapshot()
    {
        // Arrange
        var order = new Order(
            new("01961f3a-0000-7000-8000-000000000002"),
            "Jane Smith",
            249.50m,
            Status.Completed
        )
        {
            CreatedAt = new(2025, 8, 20),
        };

        // Act
        var html = await _renderer.RenderAsync(order, "Orders/OrderEmail", CancellationToken.None);

        // Assert
        await Verify(html, "html").ScrubLinesContaining("© ");
    }

    [Test]
    public async Task GivenCanceledOrder_WhenRenderingOrderEmail_ThenShouldMatchSnapshot()
    {
        // Arrange
        var order = new Order(
            new("01961f3a-0000-7000-8000-000000000003"),
            "Bob Wilson",
            15.00m,
            Status.Canceled
        )
        {
            CreatedAt = new(2025, 12, 1),
        };

        // Act
        var html = await _renderer.RenderAsync(order, "Orders/OrderEmail", CancellationToken.None);

        // Assert
        await Verify(html, "html").ScrubLinesContaining("© ");
    }

    [Test]
    public async Task GivenOrderWithSpecialCharactersInName_WhenRenderingOrderEmail_ThenShouldMatchSnapshot()
    {
        // Arrange
        var order = new Order(
            new("01961f3a-0000-7000-8000-000000000004"),
            "José García-López",
            1_234.56m,
            Status.New
        )
        {
            CreatedAt = new(2025, 1, 1),
        };

        // Act
        var html = await _renderer.RenderAsync(order, "Orders/OrderEmail", CancellationToken.None);

        // Assert
        await Verify(html, "html").ScrubLinesContaining("© ");
    }

    [Test]
    public async Task GivenOrderWithZeroTotal_WhenRenderingOrderEmail_ThenShouldMatchSnapshot()
    {
        // Arrange
        var order = new Order(
            new("01961f3a-0000-7000-8000-000000000005"),
            "Free Promo User",
            0m,
            Status.New
        )
        {
            CreatedAt = new(2025, 3, 10),
        };

        // Act
        var html = await _renderer.RenderAsync(order, "Orders/OrderEmail", CancellationToken.None);

        // Assert
        await Verify(html, "html").ScrubLinesContaining("© ");
    }

    [Test]
    public async Task GivenNonExistentTemplate_WhenRendering_ThenShouldThrowFileNotFoundException()
    {
        // Arrange
        var order = new Order(Guid.CreateVersion7(), "Test", 10m, Status.New);

        // Act & Assert
        await Should.ThrowAsync<FileNotFoundException>(async () =>
            await _renderer.RenderAsync(order, "NonExistent/Template", CancellationToken.None)
        );
    }
}
