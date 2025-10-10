using BookWorm.Catalog.Features.Categories;
using BookWorm.Catalog.Features.Categories.List;
using Mediator;
using Microsoft.AspNetCore.Http.HttpResults;

namespace BookWorm.Catalog.UnitTests.Features.Categories.List;

public sealed class ListCategoriesEndpointTests
{
    private readonly List<CategoryDto> _categories =
    [
        new(Guid.CreateVersion7(), "Fiction"),
        new(Guid.CreateVersion7(), "Non-Fiction"),
        new(Guid.CreateVersion7(), "Science"),
    ];

    private readonly ListCategoriesEndpoint _endpoint = new();
    private readonly Mock<ISender> _senderMock = new();

    [Test]
    public async Task GivenValidRequest_WhenHandlingListCategories_ThenShouldSendQuery()
    {
        // Arrange
        _senderMock
            .Setup(x => x.Send(It.IsAny<ListCategoriesQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(_categories);

        // Act
        await _endpoint.HandleAsync(_senderMock.Object);

        // Assert
        _senderMock.Verify(
            x => x.Send(It.IsAny<ListCategoriesQuery>(), It.IsAny<CancellationToken>()),
            Times.Once
        );
    }

    [Test]
    public async Task GivenValidRequest_WhenHandlingListCategories_ThenShouldReturnOkWithCategories()
    {
        // Arrange
        _senderMock
            .Setup(x => x.Send(It.IsAny<ListCategoriesQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(_categories);

        // Act
        var result = await _endpoint.HandleAsync(_senderMock.Object);

        // Assert
        result.ShouldNotBeNull();
        result.ShouldBeOfType<Ok<IReadOnlyList<CategoryDto>>>();

        result.Value.ShouldNotBeNull();
        result.Value.Count.ShouldBe(3);
        result.Value.ShouldBe(_categories);

        foreach (var category in result.Value)
        {
            category.Id.ShouldBe(_categories.First(x => x.Id == category.Id).Id);
            category.Name.ShouldBe(_categories.First(x => x.Id == category.Id).Name);
        }

        _senderMock.Verify(
            x => x.Send(It.IsAny<ListCategoriesQuery>(), It.IsAny<CancellationToken>()),
            Times.Once
        );
    }
}
