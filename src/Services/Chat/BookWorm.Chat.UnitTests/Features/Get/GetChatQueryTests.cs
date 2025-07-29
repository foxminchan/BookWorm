using BookWorm.Chassis.CQRS.Query;
using BookWorm.Chassis.Exceptions;
using BookWorm.Chat.Domain.AggregatesModel;
using BookWorm.Chat.Features;
using BookWorm.Chat.Features.Get;
using BookWorm.Chat.UnitTests.Fakers;

namespace BookWorm.Chat.UnitTests.Features.Get;

public sealed class GetChatQueryTests
{
    private readonly ConversationFaker _conversationFaker;
    private readonly GetChatHandler _handler;
    private readonly Mock<IConversationRepository> _repositoryMock;

    public GetChatQueryTests()
    {
        _repositoryMock = new();
        _handler = new(_repositoryMock.Object);
        _conversationFaker = new();
    }

    [Test]
    public void GivenConversationId_WhenCreatingGetChatQuery_ThenPropertiesShouldBeCorrectlyInitialized()
    {
        // Arrange
        var conversationId = Guid.CreateVersion7();

        // Act
        var query = new GetChatQuery(conversationId);

        // Assert
        query.ShouldNotBeNull();
        query.Id.ShouldBe(conversationId);
        query.ShouldBeOfType<GetChatQuery>();
        query.ShouldBeAssignableTo<IQuery<ConversationDto>>();
    }

    [Test]
    public void GivenTwoGetChatQueriesWithSameId_WhenComparing_ThenShouldBeEqual()
    {
        // Arrange
        var conversationId = Guid.CreateVersion7();
        var query1 = new GetChatQuery(conversationId);
        var query2 = new GetChatQuery(conversationId);

        // Act & Assert
        query1.ShouldBe(query2);
        query1.GetHashCode().ShouldBe(query2.GetHashCode());
        (query1 == query2).ShouldBeTrue();
        (query1 != query2).ShouldBeFalse();
    }

    [Test]
    public void GivenTwoGetChatQueriesWithDifferentIds_WhenComparing_ThenShouldNotBeEqual()
    {
        // Arrange
        var conversationId1 = Guid.CreateVersion7();
        var conversationId2 = Guid.CreateVersion7();
        var query1 = new GetChatQuery(conversationId1);
        var query2 = new GetChatQuery(conversationId2);

        // Act & Assert
        query1.ShouldNotBe(query2);
        query1.GetHashCode().ShouldNotBe(query2.GetHashCode());
        (query1 == query2).ShouldBeFalse();
        (query1 != query2).ShouldBeTrue();
    }

    [Test]
    public async Task GivenExistingConversationId_WhenHandlingGetChatQuery_ThenShouldReturnConversationDto()
    {
        // Arrange
        var conversation = _conversationFaker.Generate(1)[0];
        var query = new GetChatQuery(conversation.Id);

        _repositoryMock
            .Setup(r => r.GetByIdAsync(conversation.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(conversation);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.ShouldNotBeNull();
        result.Id.ShouldBe(conversation.Id);
        result.Name.ShouldBe(conversation.Name);
        result.UserId.ShouldBe(conversation.UserId);
        result.Messages.ShouldNotBeNull();
        result.Messages.Count.ShouldBe(conversation.Messages.Count);

        _repositoryMock.Verify(
            r => r.GetByIdAsync(conversation.Id, It.IsAny<CancellationToken>()),
            Times.Once
        );
    }

    [Test]
    public async Task GivenConversationWithMessages_WhenHandlingGetChatQuery_ThenShouldReturnCompleteConversationDto()
    {
        // Arrange
        var conversation = _conversationFaker.Generate(1)[0];
        var messagesFaker = new ConversationMessageFaker();
        var messages = messagesFaker.Generate();

        // Add messages to conversation using reflection since AddMessage is public
        foreach (var message in messages)
        {
            conversation.AddMessage(message);
        }

        var query = new GetChatQuery(conversation.Id);

        _repositoryMock
            .Setup(r => r.GetByIdAsync(conversation.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(conversation);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.ShouldNotBeNull();
        result.Id.ShouldBe(conversation.Id);
        result.Name.ShouldBe(conversation.Name);
        result.UserId.ShouldBe(conversation.UserId);
        result.Messages.Count.ShouldBe(messages.Length);

        for (var i = 0; i < messages.Length; i++)
        {
            result.Messages[i].Id.ShouldBe(messages[i].Id);
            result.Messages[i].Text.ShouldBe(messages[i].Text);
            result.Messages[i].Role.ShouldBe(messages[i].Role);
            result.Messages[i].ParentMessageId.ShouldBe(messages[i].ParentMessageId);
            result.Messages[i].CreatedAt.ShouldBe(messages[i].CreatedAt);
        }

        _repositoryMock.Verify(
            r => r.GetByIdAsync(conversation.Id, It.IsAny<CancellationToken>()),
            Times.Once
        );
    }

    [Test]
    public async Task GivenNonExistentConversationId_WhenHandlingGetChatQuery_ThenShouldThrowNotFoundException()
    {
        // Arrange
        var nonExistentId = Guid.CreateVersion7();
        var query = new GetChatQuery(nonExistentId);

        _repositoryMock
            .Setup(r => r.GetByIdAsync(nonExistentId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Conversation?)null);

        // Act
        var act = async () => await _handler.Handle(query, CancellationToken.None);

        // Assert
        var exception = await act.ShouldThrowAsync<NotFoundException>();
        exception.Message.ShouldBe($"Conversation with id {nonExistentId} not found.");

        _repositoryMock.Verify(
            r => r.GetByIdAsync(nonExistentId, It.IsAny<CancellationToken>()),
            Times.Once
        );
    }

    [Test]
    public async Task GivenEmptyGuidConversationId_WhenHandlingGetChatQuery_ThenShouldCallRepositoryWithEmptyGuid()
    {
        // Arrange
        var emptyId = Guid.Empty;
        var query = new GetChatQuery(emptyId);

        _repositoryMock
            .Setup(r => r.GetByIdAsync(emptyId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Conversation?)null);

        // Act
        var act = async () => await _handler.Handle(query, CancellationToken.None);

        // Assert
        await act.ShouldThrowAsync<NotFoundException>();
        _repositoryMock.Verify(
            r => r.GetByIdAsync(emptyId, It.IsAny<CancellationToken>()),
            Times.Once
        );
    }

    [Test]
    public async Task GivenCancellationTokenRequested_WhenHandlingGetChatQuery_ThenShouldPassTokenToRepository()
    {
        // Arrange
        var conversation = _conversationFaker.Generate(1)[0];
        var query = new GetChatQuery(conversation.Id);
        using var cancellationTokenSource = new CancellationTokenSource();
        var cancellationToken = cancellationTokenSource.Token;

        _repositoryMock
            .Setup(r => r.GetByIdAsync(conversation.Id, cancellationToken))
            .ReturnsAsync(conversation);

        // Act
        var result = await _handler.Handle(query, cancellationToken);

        // Assert
        result.ShouldNotBeNull();
        _repositoryMock.Verify(r => r.GetByIdAsync(conversation.Id, cancellationToken), Times.Once);
    }

    [Test]
    public async Task GivenRepositoryThrowsException_WhenHandlingGetChatQuery_ThenShouldPropagateException()
    {
        // Arrange
        var conversationId = Guid.CreateVersion7();
        var query = new GetChatQuery(conversationId);
        var expectedException = new InvalidOperationException("Database connection failed");

        _repositoryMock
            .Setup(r => r.GetByIdAsync(conversationId, It.IsAny<CancellationToken>()))
            .ThrowsAsync(expectedException);

        // Act
        var act = async () => await _handler.Handle(query, CancellationToken.None);

        // Assert
        var exception = await act.ShouldThrowAsync<InvalidOperationException>();
        exception.Message.ShouldBe("Database connection failed");
        exception.ShouldBe(expectedException);

        _repositoryMock.Verify(
            r => r.GetByIdAsync(conversationId, It.IsAny<CancellationToken>()),
            Times.Once
        );
    }

    [Test]
    public async Task GivenMultipleSequentialQueries_WhenHandlingGetChatQuery_ThenShouldHandleEachQueryIndependently()
    {
        // Arrange
        var conversations = _conversationFaker.Generate(3);
        var query1 = new GetChatQuery(conversations[0].Id);
        var query2 = new GetChatQuery(conversations[1].Id);
        var query3 = new GetChatQuery(conversations[2].Id);

        _repositoryMock
            .Setup(r => r.GetByIdAsync(conversations[0].Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(conversations[0]);
        _repositoryMock
            .Setup(r => r.GetByIdAsync(conversations[1].Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(conversations[1]);
        _repositoryMock
            .Setup(r => r.GetByIdAsync(conversations[2].Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(conversations[2]);

        // Act
        var result1 = await _handler.Handle(query1, CancellationToken.None);
        var result2 = await _handler.Handle(query2, CancellationToken.None);
        var result3 = await _handler.Handle(query3, CancellationToken.None);

        // Assert
        result1.Id.ShouldBe(conversations[0].Id);
        result2.Id.ShouldBe(conversations[1].Id);
        result3.Id.ShouldBe(conversations[2].Id);

        _repositoryMock.Verify(
            r => r.GetByIdAsync(conversations[0].Id, It.IsAny<CancellationToken>()),
            Times.Once
        );
        _repositoryMock.Verify(
            r => r.GetByIdAsync(conversations[1].Id, It.IsAny<CancellationToken>()),
            Times.Once
        );
        _repositoryMock.Verify(
            r => r.GetByIdAsync(conversations[2].Id, It.IsAny<CancellationToken>()),
            Times.Once
        );
    }

    [Test]
    public void GivenGetChatHandler_WhenCheckingImplementedInterfaces_ThenShouldImplementCorrectInterface()
    {
        // Act & Assert
        _handler.ShouldNotBeNull();
        _handler.ShouldBeOfType<GetChatHandler>();
        _handler.ShouldBeAssignableTo<IQueryHandler<GetChatQuery, ConversationDto>>();
    }

    [Test]
    public async Task GivenConversationWithNullUserId_WhenHandlingGetChatQuery_ThenShouldReturnConversationDtoWithNullUserId()
    {
        // Arrange
        var conversation = new Conversation("Test Conversation", null); // Null userId
        var query = new GetChatQuery(conversation.Id);

        _repositoryMock
            .Setup(r => r.GetByIdAsync(conversation.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(conversation);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.ShouldNotBeNull();
        result.Id.ShouldBe(conversation.Id);
        result.Name.ShouldBe(conversation.Name);
        result.UserId.ShouldBeNull();
        result.Messages.ShouldBeEmpty();

        _repositoryMock.Verify(
            r => r.GetByIdAsync(conversation.Id, It.IsAny<CancellationToken>()),
            Times.Once
        );
    }

    [Test]
    public async Task GivenConversationWithEmptyMessages_WhenHandlingGetChatQuery_ThenShouldReturnConversationDtoWithEmptyMessages()
    {
        // Arrange
        var conversation = _conversationFaker.Generate(1)[0];
        var query = new GetChatQuery(conversation.Id);

        _repositoryMock
            .Setup(r => r.GetByIdAsync(conversation.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(conversation);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.ShouldNotBeNull();
        result.Id.ShouldBe(conversation.Id);
        result.Messages.ShouldBeEmpty();

        _repositoryMock.Verify(
            r => r.GetByIdAsync(conversation.Id, It.IsAny<CancellationToken>()),
            Times.Once
        );
    }
}
