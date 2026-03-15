using BookWorm.Catalog.Domain.AggregatesModel.BookAggregate.Specifications;

namespace BookWorm.Catalog.UnitTests.Domain;

public sealed class BookAuthorFilterSpecTests
{
    [Test]
    public void GivenAuthorId_WhenCreatingSpec_ThenShouldApplyWhereFilter()
    {
        // Arrange
        var authorId = Guid.CreateVersion7();

        // Act
        var spec = new BookAuthorFilterSpec(authorId);

        // Assert
        spec.WhereExpressions.ShouldNotBeEmpty();
    }
}
