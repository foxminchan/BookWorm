using BookWorm.McpTools.Models;
using Vogen;

namespace BookWorm.McpTools.UnitTests.Models;

public sealed class ModelIdTests
{
    [Test]
    public void GivenEmptyGuid_WhenCreatingBookId_ThenShouldThrowValueObjectValidationException()
    {
        Should.Throw<ValueObjectValidationException>(() => BookId.From(Guid.Empty));
    }

    [Test]
    public void GivenEmptyGuid_WhenCreatingAuthorId_ThenShouldThrowValueObjectValidationException()
    {
        Should.Throw<ValueObjectValidationException>(() => AuthorId.From(Guid.Empty));
    }

    [Test]
    public void GivenEmptyGuid_WhenCreatingCategoryId_ThenShouldThrowValueObjectValidationException()
    {
        Should.Throw<ValueObjectValidationException>(() => CategoryId.From(Guid.Empty));
    }

    [Test]
    public void GivenEmptyGuid_WhenCreatingPublisherId_ThenShouldThrowValueObjectValidationException()
    {
        Should.Throw<ValueObjectValidationException>(() => PublisherId.From(Guid.Empty));
    }
}
