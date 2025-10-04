using BookWorm.Chat.Features;

namespace BookWorm.Chat.UnitTests.Features;

public sealed class StreamContextTests
{
    [Test]
    public void GivenBothParameters_WhenCreatingStreamContext_ThenShouldCreateSuccessfully()
    {
        // Arrange
        var lastMessageId = Guid.CreateVersion7();
        var lastFragmentId = Guid.CreateVersion7();

        // Act
        var context = new StreamContext(lastMessageId, lastFragmentId);

        // Assert
        context.ShouldNotBeNull();
        context.LastMessageId.ShouldBe(lastMessageId);
        context.LastFragmentId.ShouldBe(lastFragmentId);
    }

    [Test]
    public void GivenNullLastMessageId_WhenCreatingStreamContext_ThenShouldCreateSuccessfully()
    {
        // Arrange
        var lastFragmentId = Guid.CreateVersion7();

        // Act
        var context = new StreamContext(null, lastFragmentId);

        // Assert
        context.LastMessageId.ShouldBeNull();
        context.LastFragmentId.ShouldBe(lastFragmentId);
    }

    [Test]
    public void GivenNullLastFragmentId_WhenCreatingStreamContext_ThenShouldCreateSuccessfully()
    {
        // Arrange
        var lastMessageId = Guid.CreateVersion7();

        // Act
        var context = new StreamContext(lastMessageId, null);

        // Assert
        context.LastMessageId.ShouldBe(lastMessageId);
        context.LastFragmentId.ShouldBeNull();
    }

    [Test]
    public void GivenBothParametersNull_WhenCreatingStreamContext_ThenShouldCreateSuccessfully()
    {
        // Act
        var context = new StreamContext(null, null);

        // Assert
        context.LastMessageId.ShouldBeNull();
        context.LastFragmentId.ShouldBeNull();
    }

    [Test]
    public void GivenTwoContextsWithSameValues_WhenComparing_ThenShouldBeEqual()
    {
        // Arrange
        var lastMessageId = Guid.CreateVersion7();
        var lastFragmentId = Guid.CreateVersion7();

        var context1 = new StreamContext(lastMessageId, lastFragmentId);
        var context2 = new StreamContext(lastMessageId, lastFragmentId);

        // Act & Assert
        context1.ShouldBe(context2);
        context1.GetHashCode().ShouldBe(context2.GetHashCode());
        (context1 == context2).ShouldBeTrue();
        (context1 != context2).ShouldBeFalse();
    }

    [Test]
    public void GivenTwoContextsWithBothNull_WhenComparing_ThenShouldBeEqual()
    {
        // Arrange
        var context1 = new StreamContext(null, null);
        var context2 = new StreamContext(null, null);

        // Act & Assert
        context1.ShouldBe(context2);
        context1.GetHashCode().ShouldBe(context2.GetHashCode());
    }

    [Test]
    public void GivenTwoContextsWithDifferentLastMessageId_WhenComparing_ThenShouldNotBeEqual()
    {
        // Arrange
        var lastMessageId1 = Guid.CreateVersion7();
        var lastMessageId2 = Guid.CreateVersion7();
        var lastFragmentId = Guid.CreateVersion7();

        var context1 = new StreamContext(lastMessageId1, lastFragmentId);
        var context2 = new StreamContext(lastMessageId2, lastFragmentId);

        // Act & Assert
        context1.ShouldNotBe(context2);
        (context1 == context2).ShouldBeFalse();
        (context1 != context2).ShouldBeTrue();
    }

    [Test]
    public void GivenTwoContextsWithDifferentLastFragmentId_WhenComparing_ThenShouldNotBeEqual()
    {
        // Arrange
        var lastMessageId = Guid.CreateVersion7();
        var lastFragmentId1 = Guid.CreateVersion7();
        var lastFragmentId2 = Guid.CreateVersion7();

        var context1 = new StreamContext(lastMessageId, lastFragmentId1);
        var context2 = new StreamContext(lastMessageId, lastFragmentId2);

        // Act & Assert
        context1.ShouldNotBe(context2);
    }

    [Test]
    public void GivenContextWithNullAndContextWithValue_WhenComparing_ThenShouldNotBeEqual()
    {
        // Arrange
        var lastMessageId = Guid.CreateVersion7();
        var lastFragmentId = Guid.CreateVersion7();

        var context1 = new StreamContext(null, null);
        var context2 = new StreamContext(lastMessageId, lastFragmentId);

        // Act & Assert
        context1.ShouldNotBe(context2);
    }

    [Test]
    public void GivenContextWithOneNullParameter_WhenComparing_ThenShouldNotBeEqual()
    {
        // Arrange
        var lastMessageId = Guid.CreateVersion7();
        var lastFragmentId = Guid.CreateVersion7();

        var context1 = new StreamContext(lastMessageId, null);
        var context2 = new StreamContext(lastMessageId, lastFragmentId);

        // Act & Assert
        context1.ShouldNotBe(context2);
    }

    [Test]
    public void GivenEmptyGuids_WhenCreatingStreamContext_ThenShouldCreateSuccessfully()
    {
        // Act
        var context = new StreamContext(Guid.Empty, Guid.Empty);

        // Assert
        context.LastMessageId.ShouldBe(Guid.Empty);
        context.LastFragmentId.ShouldBe(Guid.Empty);
    }

    [Test]
    public void GivenStreamContext_WhenCheckingType_ThenShouldBeCorrectType()
    {
        // Arrange & Act
        var context = new StreamContext(Guid.CreateVersion7(), Guid.CreateVersion7());

        // Assert
        context.ShouldBeOfType<StreamContext>();
        context.ShouldBeAssignableTo<StreamContext>();
    }

    [Test]
    public void GivenStreamContext_WhenDeconstructing_ThenShouldDeconstructCorrectly()
    {
        // Arrange
        var expectedLastMessageId = Guid.CreateVersion7();
        var expectedLastFragmentId = Guid.CreateVersion7();

        var context = new StreamContext(expectedLastMessageId, expectedLastFragmentId);

        // Act
        var (lastMessageId, lastFragmentId) = context;

        // Assert
        lastMessageId.ShouldBe(expectedLastMessageId);
        lastFragmentId.ShouldBe(expectedLastFragmentId);
    }

    [Test]
    public void GivenStreamContextWithNulls_WhenDeconstructing_ThenShouldDeconstructCorrectly()
    {
        // Arrange
        var context = new StreamContext(null, null);

        // Act
        var (lastMessageId, lastFragmentId) = context;

        // Assert
        lastMessageId.ShouldBeNull();
        lastFragmentId.ShouldBeNull();
    }

    [Test]
    public void GivenStreamContextWithMixedNulls_WhenDeconstructing_ThenShouldDeconstructCorrectly()
    {
        // Arrange
        var expectedLastMessageId = Guid.CreateVersion7();
        var context = new StreamContext(expectedLastMessageId, null);

        // Act
        var (lastMessageId, lastFragmentId) = context;

        // Assert
        lastMessageId.ShouldBe(expectedLastMessageId);
        lastFragmentId.ShouldBeNull();
    }

    [Test]
    public void GivenStreamContextAsInitialState_WhenUsedForStreaming_ThenShouldIndicateStartFromBeginning()
    {
        // Arrange & Act
        var context = new StreamContext(null, null);

        // Assert
        context.LastMessageId.ShouldBeNull();
        context.LastFragmentId.ShouldBeNull();
    }

    [Test]
    public void GivenStreamContextWithLastMessageId_WhenUsedForStreaming_ThenShouldIndicateResumePoint()
    {
        // Arrange
        var resumeFromMessageId = Guid.CreateVersion7();

        // Act
        var context = new StreamContext(resumeFromMessageId, null);

        // Assert
        context.LastMessageId.ShouldBe(resumeFromMessageId);
        context.LastFragmentId.ShouldBeNull();
    }

    [Test]
    public void GivenStreamContextWithBothIds_WhenUsedForStreaming_ThenShouldIndicatePreciseResumePoint()
    {
        // Arrange
        var resumeFromMessageId = Guid.CreateVersion7();
        var resumeFromFragmentId = Guid.CreateVersion7();

        // Act
        var context = new StreamContext(resumeFromMessageId, resumeFromFragmentId);

        // Assert
        context.LastMessageId.ShouldBe(resumeFromMessageId);
        context.LastFragmentId.ShouldBe(resumeFromFragmentId);
    }
}
