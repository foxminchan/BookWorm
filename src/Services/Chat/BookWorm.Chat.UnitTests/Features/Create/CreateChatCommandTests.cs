using System.Security.Claims;
using BookWorm.Chassis.Repository;
using BookWorm.Chat.Domain.AggregatesModel;
using BookWorm.Chat.Features.Create;

namespace BookWorm.Chat.UnitTests.Features.Create;

public sealed class CreateChatCommandTests
{
    private readonly Mock<ClaimsPrincipal> _claimsPrincipalMock;
    private readonly CreateChatHandler _handler;
    private readonly Mock<IConversationRepository> _repositoryMock;
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;
    private readonly Guid _userId;

    public CreateChatCommandTests()
    {
        _repositoryMock = new();
        _claimsPrincipalMock = new();
        _unitOfWorkMock = new();
        _userId = Guid.CreateVersion7();

        // Setup unit of work
        _repositoryMock.Setup(x => x.UnitOfWork).Returns(_unitOfWorkMock.Object);

        _handler = new(_repositoryMock.Object, _claimsPrincipalMock.Object);
    }

    [Test]
    public async Task GivenValidCommand_WhenHandlingCreateChat_ThenShouldCreateConversationAndReturnId()
    {
        // Arrange
        var command = new CreateChatCommand("Test Chat");
        var conversation = new Conversation("Test Chat", _userId);

        SetupClaimsPrincipal(_userId);
        SetupRepositoryAddAsync(conversation);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.ShouldBe(Guid.Empty);
        _repositoryMock.Verify(
            x =>
                x.AddAsync(
                    It.Is<Conversation>(c => c.Name == "Test Chat" && c.UserId == _userId),
                    It.IsAny<CancellationToken>()
                ),
            Times.Once
        );
        _unitOfWorkMock.Verify(x => x.SaveEntitiesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Test]
    public async Task GivenValidCommandWithLongName_WhenHandlingCreateChat_ThenShouldCreateConversationSuccessfully()
    {
        // Arrange
        var longName = new string('A', 500);
        var command = new CreateChatCommand(longName);
        var conversation = new Conversation(longName, _userId);

        SetupClaimsPrincipal(_userId);
        SetupRepositoryAddAsync(conversation);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.ShouldBe(Guid.Empty);
        _repositoryMock.Verify(
            x =>
                x.AddAsync(
                    It.Is<Conversation>(c => c.Name == longName && c.UserId == _userId),
                    It.IsAny<CancellationToken>()
                ),
            Times.Once
        );
    }

    [Test]
    public async Task GivenValidCommand_WhenHandlingCreateChat_ThenShouldCallSaveEntitiesAsync()
    {
        // Arrange
        var command = new CreateChatCommand("Test Chat");
        var conversation = new Conversation("Test Chat", _userId);

        SetupClaimsPrincipal(_userId);
        SetupRepositoryAddAsync(conversation);

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert
        _unitOfWorkMock.Verify(x => x.SaveEntitiesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Test]
    public async Task GivenCancellationTokenRequested_WhenHandlingCreateChat_ThenShouldPassTokenToRepository()
    {
        // Arrange
        var command = new CreateChatCommand("Test Chat");
        var conversation = new Conversation("Test Chat", _userId);
        using var cancellationTokenSource = new CancellationTokenSource();
        var cancellationToken = cancellationTokenSource.Token;

        SetupClaimsPrincipal(_userId);
        SetupRepositoryAddAsync(conversation);

        // Act
        await _handler.Handle(command, cancellationToken);

        // Assert
        _repositoryMock.Verify(
            x => x.AddAsync(It.IsAny<Conversation>(), cancellationToken),
            Times.Once
        );
        _unitOfWorkMock.Verify(x => x.SaveEntitiesAsync(cancellationToken), Times.Once);
    }

    [Test]
    public async Task GivenMissingUserIdClaim_WhenHandlingCreateChat_ThenShouldThrowUnauthorizedAccessException()
    {
        // Arrange
        var command = new CreateChatCommand("Test Chat");

        _claimsPrincipalMock
            .Setup(x => x.FindFirst(ClaimTypes.NameIdentifier))
            .Returns((Claim?)null);

        // Act & Assert
        var exception = await Should.ThrowAsync<UnauthorizedAccessException>(() =>
            _handler.Handle(command, CancellationToken.None)
        );

        exception.Message.ShouldBe("User is not authenticated.");
        _repositoryMock.Verify(
            x => x.AddAsync(It.IsAny<Conversation>(), It.IsAny<CancellationToken>()),
            Times.Never
        );
    }

    [Test]
    public async Task GivenEmptyUserIdClaim_WhenHandlingCreateChat_ThenShouldThrowUnauthorizedAccessException()
    {
        // Arrange
        var command = new CreateChatCommand("Test Chat");

        _claimsPrincipalMock
            .Setup(x => x.FindFirst(ClaimTypes.NameIdentifier))
            .Returns(new Claim(ClaimTypes.NameIdentifier, string.Empty));

        // Act & Assert
        var exception = await Should.ThrowAsync<UnauthorizedAccessException>(() =>
            _handler.Handle(command, CancellationToken.None)
        );

        exception.Message.ShouldBe("User is not authenticated.");
    }

    [Test]
    public async Task GivenWhitespaceUserIdClaim_WhenHandlingCreateChat_ThenShouldThrowUnauthorizedAccessException()
    {
        // Arrange
        var command = new CreateChatCommand("Test Chat");

        _claimsPrincipalMock
            .Setup(x => x.FindFirst(ClaimTypes.NameIdentifier))
            .Returns(new Claim(ClaimTypes.NameIdentifier, "   "));

        // Act & Assert
        var exception = await Should.ThrowAsync<UnauthorizedAccessException>(() =>
            _handler.Handle(command, CancellationToken.None)
        );

        exception.Message.ShouldBe("User is not authenticated.");
    }

    [Test]
    public async Task GivenInvalidUserIdFormat_WhenHandlingCreateChat_ThenShouldThrowArgumentException()
    {
        // Arrange
        var command = new CreateChatCommand("Test Chat");

        _claimsPrincipalMock
            .Setup(x => x.FindFirst(ClaimTypes.NameIdentifier))
            .Returns(new Claim(ClaimTypes.NameIdentifier, "invalid-guid"));

        // Act & Assert
        var exception = await Should.ThrowAsync<ArgumentException>(() =>
            _handler.Handle(command, CancellationToken.None)
        );

        exception.Message.ShouldBe("Invalid User ID format. (Parameter 'userId')");
        exception.ParamName.ShouldBe("userId");
    }

    [Test]
    public async Task GivenRepositoryThrowsException_WhenHandlingCreateChat_ThenShouldPropagateException()
    {
        // Arrange
        var command = new CreateChatCommand("Test Chat");
        var expectedException = new InvalidOperationException("Database error");

        SetupClaimsPrincipal(_userId);
        _repositoryMock
            .Setup(x => x.AddAsync(It.IsAny<Conversation>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(expectedException);

        // Act & Assert
        var exception = await Should.ThrowAsync<InvalidOperationException>(() =>
            _handler.Handle(command, CancellationToken.None)
        );

        exception.ShouldBe(expectedException);
        _unitOfWorkMock.Verify(
            x => x.SaveEntitiesAsync(It.IsAny<CancellationToken>()),
            Times.Never
        );
    }

    [Test]
    public async Task GivenUnitOfWorkThrowsException_WhenHandlingCreateChat_ThenShouldPropagateException()
    {
        // Arrange
        var command = new CreateChatCommand("Test Chat");
        var conversation = new Conversation("Test Chat", _userId);
        var expectedException = new InvalidOperationException("Save error");

        SetupClaimsPrincipal(_userId);
        SetupRepositoryAddAsync(conversation);
        _unitOfWorkMock
            .Setup(x => x.SaveEntitiesAsync(It.IsAny<CancellationToken>()))
            .ThrowsAsync(expectedException);

        // Act & Assert
        var exception = await Should.ThrowAsync<InvalidOperationException>(() =>
            _handler.Handle(command, CancellationToken.None)
        );

        exception.ShouldBe(expectedException);
        _repositoryMock.Verify(
            x => x.AddAsync(It.IsAny<Conversation>(), It.IsAny<CancellationToken>()),
            Times.Once
        );
    }

    [Test]
    public void GivenChatName_WhenCreatingCreateChatCommand_ThenPropertiesShouldBeCorrectlyInitialized()
    {
        // Arrange & Act
        var command = new CreateChatCommand("Test Chat");

        // Assert
        command.Name.ShouldBe("Test Chat");
    }

    [Test]
    public void GivenTwoCreateChatCommandsWithSameName_WhenComparing_ThenShouldBeEqual()
    {
        // Arrange
        var command1 = new CreateChatCommand("Test Chat");
        var command2 = new CreateChatCommand("Test Chat");

        // Act & Assert
        command1.ShouldBe(command2);
        command1.GetHashCode().ShouldBe(command2.GetHashCode());
    }

    [Test]
    public void GivenTwoCreateChatCommandsWithDifferentNames_WhenComparing_ThenShouldNotBeEqual()
    {
        // Arrange
        var command1 = new CreateChatCommand("Test Chat 1");
        var command2 = new CreateChatCommand("Test Chat 2");

        // Act & Assert
        command1.ShouldNotBe(command2);
        command1.GetHashCode().ShouldNotBe(command2.GetHashCode());
    }

    [Test]
    public async Task GivenMultipleSequentialCommands_WhenHandlingCreateChat_ThenShouldCreateEachConversation()
    {
        // Arrange
        var command1 = new CreateChatCommand("Chat 1");
        var command2 = new CreateChatCommand("Chat 2");
        var command3 = new CreateChatCommand("Chat 3");

        SetupClaimsPrincipal(_userId);
        _repositoryMock
            .Setup(x => x.AddAsync(It.IsAny<Conversation>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Conversation c, CancellationToken _) => c);

        // Act
        var result1 = await _handler.Handle(command1, CancellationToken.None);
        var result2 = await _handler.Handle(command2, CancellationToken.None);
        var result3 = await _handler.Handle(command3, CancellationToken.None);

        // Assert
        result1.ShouldBe(Guid.Empty);
        result2.ShouldBe(Guid.Empty);
        result3.ShouldBe(Guid.Empty);

        _repositoryMock.Verify(
            x =>
                x.AddAsync(
                    It.Is<Conversation>(c => c.Name == "Chat 1"),
                    It.IsAny<CancellationToken>()
                ),
            Times.Once
        );
        _repositoryMock.Verify(
            x =>
                x.AddAsync(
                    It.Is<Conversation>(c => c.Name == "Chat 2"),
                    It.IsAny<CancellationToken>()
                ),
            Times.Once
        );
        _repositoryMock.Verify(
            x =>
                x.AddAsync(
                    It.Is<Conversation>(c => c.Name == "Chat 3"),
                    It.IsAny<CancellationToken>()
                ),
            Times.Once
        );
        _unitOfWorkMock.Verify(
            x => x.SaveEntitiesAsync(It.IsAny<CancellationToken>()),
            Times.Exactly(3)
        );
    }

    [Test]
    public async Task GivenValidCommandWithSpecialCharacters_WhenHandlingCreateChat_ThenShouldCreateConversationSuccessfully()
    {
        // Arrange
        var chatName = "Chat with émojis 🚀 and spëcial chars!";
        var command = new CreateChatCommand(chatName);
        var conversation = new Conversation(chatName, _userId);

        SetupClaimsPrincipal(_userId);
        SetupRepositoryAddAsync(conversation);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.ShouldBe(Guid.Empty);
        _repositoryMock.Verify(
            x =>
                x.AddAsync(
                    It.Is<Conversation>(c => c.Name == chatName && c.UserId == _userId),
                    It.IsAny<CancellationToken>()
                ),
            Times.Once
        );
    }

    [Test]
    public async Task GivenValidCommand_WhenHandlingCreateChat_ThenShouldReturnNewConversationId()
    {
        // Arrange
        var command = new CreateChatCommand("Test Chat");
        var conversation = new Conversation("Test Chat", _userId);

        SetupClaimsPrincipal(_userId);
        SetupRepositoryAddAsync(conversation);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.ShouldBe(conversation.Id);
    }

    private void SetupClaimsPrincipal(Guid userId)
    {
        _claimsPrincipalMock
            .Setup(x => x.FindFirst(ClaimTypes.NameIdentifier))
            .Returns(new Claim(ClaimTypes.NameIdentifier, userId.ToString()));
    }

    private void SetupRepositoryAddAsync(Conversation conversation)
    {
        _repositoryMock
            .Setup(x => x.AddAsync(It.IsAny<Conversation>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(conversation);
    }
}
