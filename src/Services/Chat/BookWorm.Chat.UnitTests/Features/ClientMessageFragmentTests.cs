using BookWorm.Chat.Features;

namespace BookWorm.Chat.UnitTests.Features;

public sealed class ClientMessageFragmentTests
{
    [Test]
    public void GivenValidParameters_WhenCreatingClientMessageFragment_ThenShouldCreateSuccessfully()
    {
        // Arrange
        var id = Guid.CreateVersion7();
        const string sender = "Assistant";
        const string text = "Hello, how can I help you?";
        var fragmentId = Guid.CreateVersion7();
        const bool isFinal = false;

        // Act
        var fragment = new ClientMessageFragment(id, sender, text, fragmentId);

        // Assert
        fragment.ShouldNotBeNull();
        fragment.Id.ShouldBe(id);
        fragment.Sender.ShouldBe(sender);
        fragment.Text.ShouldBe(text);
        fragment.FragmentId.ShouldBe(fragmentId);
        fragment.IsFinal.ShouldBe(isFinal);
    }

    [Test]
    public void GivenNoIsFinalParameter_WhenCreatingClientMessageFragment_ThenShouldDefaultToFalse()
    {
        // Arrange
        var id = Guid.CreateVersion7();
        const string sender = "User";
        const string text = "What books do you recommend?";
        var fragmentId = Guid.CreateVersion7();

        // Act
        var fragment = new ClientMessageFragment(id, sender, text, fragmentId);

        // Assert
        fragment.IsFinal.ShouldBe(false);
    }

    [Test]
    public void GivenIsFinalTrue_WhenCreatingClientMessageFragment_ThenShouldSetCorrectly()
    {
        // Arrange
        var id = Guid.CreateVersion7();
        const string sender = "Assistant";
        const string text = "Thank you for your question.";
        var fragmentId = Guid.CreateVersion7();
        const bool isFinal = true;

        // Act
        var fragment = new ClientMessageFragment(id, sender, text, fragmentId, isFinal);

        // Assert
        fragment.IsFinal.ShouldBe(true);
    }

    [Test]
    public void GivenTwoFragmentsWithSameValues_WhenComparing_ThenShouldBeEqual()
    {
        // Arrange
        var id = Guid.CreateVersion7();
        const string sender = "Assistant";
        const string text = "Test message";
        var fragmentId = Guid.CreateVersion7();
        const bool isFinal = true;

        var fragment1 = new ClientMessageFragment(id, sender, text, fragmentId, isFinal);
        var fragment2 = new ClientMessageFragment(id, sender, text, fragmentId, isFinal);

        // Act & Assert
        fragment1.ShouldBe(fragment2);
        fragment1.GetHashCode().ShouldBe(fragment2.GetHashCode());
        (fragment1 == fragment2).ShouldBeTrue();
        (fragment1 != fragment2).ShouldBeFalse();
    }

    [Test]
    public void GivenTwoFragmentsWithDifferentIds_WhenComparing_ThenShouldNotBeEqual()
    {
        // Arrange
        var id1 = Guid.CreateVersion7();
        var id2 = Guid.CreateVersion7();
        const string sender = "Assistant";
        const string text = "Test message";
        var fragmentId = Guid.CreateVersion7();

        var fragment1 = new ClientMessageFragment(id1, sender, text, fragmentId);
        var fragment2 = new ClientMessageFragment(id2, sender, text, fragmentId);

        // Act & Assert
        fragment1.ShouldNotBe(fragment2);
        (fragment1 == fragment2).ShouldBeFalse();
        (fragment1 != fragment2).ShouldBeTrue();
    }

    [Test]
    public void GivenTwoFragmentsWithDifferentSenders_WhenComparing_ThenShouldNotBeEqual()
    {
        // Arrange
        var id = Guid.CreateVersion7();
        const string text = "Test message";
        var fragmentId = Guid.CreateVersion7();

        var fragment1 = new ClientMessageFragment(id, "User", text, fragmentId);
        var fragment2 = new ClientMessageFragment(id, "Assistant", text, fragmentId);

        // Act & Assert
        fragment1.ShouldNotBe(fragment2);
    }

    [Test]
    public void GivenTwoFragmentsWithDifferentText_WhenComparing_ThenShouldNotBeEqual()
    {
        // Arrange
        var id = Guid.CreateVersion7();
        const string sender = "Assistant";
        var fragmentId = Guid.CreateVersion7();

        var fragment1 = new ClientMessageFragment(id, sender, "First message", fragmentId);
        var fragment2 = new ClientMessageFragment(id, sender, "Second message", fragmentId);

        // Act & Assert
        fragment1.ShouldNotBe(fragment2);
    }

    [Test]
    public void GivenTwoFragmentsWithDifferentFragmentIds_WhenComparing_ThenShouldNotBeEqual()
    {
        // Arrange
        var id = Guid.CreateVersion7();
        const string sender = "Assistant";
        const string text = "Test message";
        var fragmentId1 = Guid.CreateVersion7();
        var fragmentId2 = Guid.CreateVersion7();

        var fragment1 = new ClientMessageFragment(id, sender, text, fragmentId1);
        var fragment2 = new ClientMessageFragment(id, sender, text, fragmentId2);

        // Act & Assert
        fragment1.ShouldNotBe(fragment2);
    }

    [Test]
    public void GivenTwoFragmentsWithDifferentIsFinalValues_WhenComparing_ThenShouldNotBeEqual()
    {
        // Arrange
        var id = Guid.CreateVersion7();
        const string sender = "Assistant";
        const string text = "Test message";
        var fragmentId = Guid.CreateVersion7();

        var fragment1 = new ClientMessageFragment(id, sender, text, fragmentId);
        var fragment2 = new ClientMessageFragment(id, sender, text, fragmentId, true);

        // Act & Assert
        fragment1.ShouldNotBe(fragment2);
    }

    [Test]
    public void GivenEmptyGuids_WhenCreatingClientMessageFragment_ThenShouldCreateSuccessfully()
    {
        // Arrange & Act
        var fragment = new ClientMessageFragment(Guid.Empty, "Sender", "Text", Guid.Empty);

        // Assert
        fragment.ShouldNotBeNull();
        fragment.Id.ShouldBe(Guid.Empty);
        fragment.FragmentId.ShouldBe(Guid.Empty);
    }

    [Test]
    public void GivenEmptyStrings_WhenCreatingClientMessageFragment_ThenShouldCreateSuccessfully()
    {
        // Arrange
        var id = Guid.CreateVersion7();
        var fragmentId = Guid.CreateVersion7();

        // Act
        var fragment = new ClientMessageFragment(id, string.Empty, string.Empty, fragmentId);

        // Assert
        fragment.Sender.ShouldBe(string.Empty);
        fragment.Text.ShouldBe(string.Empty);
    }

    [Test]
    public void GivenLongText_WhenCreatingClientMessageFragment_ThenShouldCreateSuccessfully()
    {
        // Arrange
        var id = Guid.CreateVersion7();
        const string sender = "Assistant";
        var longText = new string('A', 10000);
        var fragmentId = Guid.CreateVersion7();

        // Act
        var fragment = new ClientMessageFragment(id, sender, longText, fragmentId);

        // Assert
        fragment.Text.ShouldBe(longText);
        fragment.Text.Length.ShouldBe(10000);
    }

    [Test]
    public void GivenSpecialCharactersInText_WhenCreatingClientMessageFragment_ThenShouldCreateSuccessfully()
    {
        // Arrange
        var id = Guid.CreateVersion7();
        const string sender = "User";
        const string specialText = "Hello! @#$%^&*() 你好 مرحبا 🌍";
        var fragmentId = Guid.CreateVersion7();

        // Act
        var fragment = new ClientMessageFragment(id, sender, specialText, fragmentId);

        // Assert
        fragment.Text.ShouldBe(specialText);
    }

    [Test]
    public void GivenClientMessageFragment_WhenCheckingType_ThenShouldBeCorrectType()
    {
        // Arrange & Act
        var fragment = new ClientMessageFragment(
            Guid.CreateVersion7(),
            "Sender",
            "Text",
            Guid.CreateVersion7()
        );

        // Assert
        fragment.ShouldBeOfType<ClientMessageFragment>();
        fragment.ShouldBeAssignableTo<ClientMessageFragment>();
    }

    [Test]
    public void GivenClientMessageFragment_WhenDeconstructing_ThenShouldDeconstructCorrectly()
    {
        // Arrange
        var expectedId = Guid.CreateVersion7();
        const string expectedSender = "Assistant";
        const string expectedText = "Test message";
        var expectedFragmentId = Guid.CreateVersion7();
        const bool expectedIsFinal = true;

        var fragment = new ClientMessageFragment(
            expectedId,
            expectedSender,
            expectedText,
            expectedFragmentId,
            expectedIsFinal
        );

        // Act
        var (id, sender, text, fragmentId, isFinal) = fragment;

        // Assert
        id.ShouldBe(expectedId);
        sender.ShouldBe(expectedSender);
        text.ShouldBe(expectedText);
        fragmentId.ShouldBe(expectedFragmentId);
        isFinal.ShouldBe(expectedIsFinal);
    }
}
