using BookWorm.Chassis.Endpoints;
using BookWorm.Chat.Features;
using BookWorm.Chat.Features.List;
using BookWorm.Chat.UnitTests.Fakers;
using MediatR;
using Microsoft.AspNetCore.Http.HttpResults;

namespace BookWorm.Chat.UnitTests.Features.List;

public sealed class ListChatEndpointTests
{
    private readonly ConversationDto[] _conversationDtos;
    private readonly ConversationFaker _conversationFaker;
    private readonly ListChatEndpoint _endpoint;
    private readonly ConversationMessageFaker _messageFaker;
    private readonly Mock<ISender> _senderMock;

    public ListChatEndpointTests()
    {
        _senderMock = new();
        _endpoint = new();
        _conversationFaker = new();
        _messageFaker = new();

        // Create sample ConversationDtos for testing
        _conversationDtos = GenerateTestConversationDtos();
    }

    [Test]
    public async Task GivenValidQueryWithAllParameters_WhenHandlingListChat_ThenShouldReturnOkWithConversationDtos()
    {
        // Arrange
        var query = new ListChatQuery("Test Chat", Guid.CreateVersion7(), true);
        var expectedResult = _conversationDtos.AsReadOnly();

        _senderMock
            .Setup(s => s.Send(It.IsAny<ListChatQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedResult);

        // Act
        var result = await _endpoint.HandleAsync(query, _senderMock.Object);

        // Assert
        result.ShouldBeOfType<Ok<IReadOnlyList<ConversationDto>>>();
        result.Value.ShouldBe(expectedResult);
        result.Value!.Count.ShouldBe(_conversationDtos.Length);

        _senderMock.Verify(
            s =>
                s.Send(
                    It.Is<ListChatQuery>(q =>
                        q.Name == query.Name
                        && q.UserId == query.UserId
                        && q.IncludeMessages == query.IncludeMessages
                    ),
                    It.IsAny<CancellationToken>()
                ),
            Times.Once
        );
    }

    [Test]
    public async Task GivenQueryWithNameOnly_WhenHandlingListChat_ThenShouldReturnOkWithFilteredResults()
    {
        // Arrange
        var query = new ListChatQuery("Specific Chat");
        var expectedResult = _conversationDtos.Take(2).ToList().AsReadOnly();

        _senderMock
            .Setup(s => s.Send(It.IsAny<ListChatQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedResult);

        // Act
        var result = await _endpoint.HandleAsync(query, _senderMock.Object);

        // Assert
        result.ShouldBeOfType<Ok<IReadOnlyList<ConversationDto>>>();
        result.Value.ShouldBe(expectedResult);
        result.Value!.Count.ShouldBe(2);

        _senderMock.Verify(
            s =>
                s.Send(
                    It.Is<ListChatQuery>(q =>
                        q.Name == "Specific Chat" && q.UserId == null && q.IncludeMessages == false
                    ),
                    It.IsAny<CancellationToken>()
                ),
            Times.Once
        );
    }

    [Test]
    public async Task GivenQueryWithUserIdOnly_WhenHandlingListChat_ThenShouldReturnOkWithUserSpecificResults()
    {
        // Arrange
        var userId = Guid.CreateVersion7();
        var query = new ListChatQuery(null, userId);
        var expectedResult = _conversationDtos.Take(1).ToList().AsReadOnly();

        _senderMock
            .Setup(s => s.Send(It.IsAny<ListChatQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedResult);

        // Act
        var result = await _endpoint.HandleAsync(query, _senderMock.Object);

        // Assert
        result.ShouldBeOfType<Ok<IReadOnlyList<ConversationDto>>>();
        result.Value.ShouldBe(expectedResult);

        _senderMock.Verify(
            s =>
                s.Send(
                    It.Is<ListChatQuery>(q =>
                        q.Name == null && q.UserId == userId && q.IncludeMessages == false
                    ),
                    It.IsAny<CancellationToken>()
                ),
            Times.Once
        );
    }

    [Test]
    public async Task GivenQueryWithIncludeMessagesTrue_WhenHandlingListChat_ThenShouldReturnOkWithMessagesIncluded()
    {
        // Arrange
        var query = new ListChatQuery(null, null, true);
        var conversationsWithMessages = GenerateConversationsWithMessages();

        _senderMock
            .Setup(s => s.Send(It.IsAny<ListChatQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(conversationsWithMessages);

        // Act
        var result = await _endpoint.HandleAsync(query, _senderMock.Object);

        // Assert
        result.ShouldBeOfType<Ok<IReadOnlyList<ConversationDto>>>();
        result.Value.ShouldBe(conversationsWithMessages);
        result.Value!.All(c => c.Messages.Count > 0).ShouldBeTrue();

        _senderMock.Verify(
            s =>
                s.Send(
                    It.Is<ListChatQuery>(q => q.IncludeMessages == true),
                    It.IsAny<CancellationToken>()
                ),
            Times.Once
        );
    }

    [Test]
    public async Task GivenEmptyQuery_WhenHandlingListChat_ThenShouldReturnOkWithAllConversations()
    {
        // Arrange
        var query = new ListChatQuery();
        var expectedResult = _conversationDtos.AsReadOnly();

        _senderMock
            .Setup(s => s.Send(It.IsAny<ListChatQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedResult);

        // Act
        var result = await _endpoint.HandleAsync(query, _senderMock.Object);

        // Assert
        result.ShouldBeOfType<Ok<IReadOnlyList<ConversationDto>>>();
        result.Value.ShouldBe(expectedResult);

        _senderMock.Verify(
            s =>
                s.Send(
                    It.Is<ListChatQuery>(q =>
                        q.Name == null && q.UserId == null && q.IncludeMessages == false
                    ),
                    It.IsAny<CancellationToken>()
                ),
            Times.Once
        );
    }

    [Test]
    public async Task GivenValidQueryWithCancellationToken_WhenHandlingListChat_ThenShouldPassCancellationTokenToSender()
    {
        // Arrange
        using var cancellationTokenSource = new CancellationTokenSource();
        var cancellationToken = cancellationTokenSource.Token;
        var query = new ListChatQuery("Test", Guid.CreateVersion7());
        var expectedResult = _conversationDtos.AsReadOnly();

        _senderMock
            .Setup(s => s.Send(It.IsAny<ListChatQuery>(), cancellationToken))
            .ReturnsAsync(expectedResult);

        // Act
        var result = await _endpoint.HandleAsync(query, _senderMock.Object, cancellationToken);

        // Assert
        result.ShouldBeOfType<Ok<IReadOnlyList<ConversationDto>>>();
        _senderMock.Verify(
            s =>
                s.Send(
                    It.Is<ListChatQuery>(q =>
                        q.Name == query.Name
                        && q.UserId == query.UserId
                        && q.IncludeMessages == query.IncludeMessages
                    ),
                    cancellationToken
                ),
            Times.Once
        );
    }

    [Test]
    public async Task GivenCancelledToken_WhenHandlingListChat_ThenShouldThrowOperationCancelledException()
    {
        // Arrange
        using var cancellationTokenSource = new CancellationTokenSource();
        await cancellationTokenSource.CancelAsync();
        var cancellationToken = cancellationTokenSource.Token;
        var query = new ListChatQuery("Test");

        _senderMock
            .Setup(s => s.Send(It.IsAny<ListChatQuery>(), cancellationToken))
            .ThrowsAsync(new OperationCanceledException());

        // Act & Assert
        await Should.ThrowAsync<OperationCanceledException>(async () =>
            await _endpoint.HandleAsync(query, _senderMock.Object, cancellationToken)
        );

        _senderMock.Verify(s => s.Send(It.IsAny<ListChatQuery>(), cancellationToken), Times.Once);
    }

    [Test]
    public async Task GivenSenderThrowsException_WhenHandlingListChat_ThenShouldPropagateException()
    {
        // Arrange
        var query = new ListChatQuery("Test");
        var expectedException = new InvalidOperationException("Database error");

        _senderMock
            .Setup(s => s.Send(It.IsAny<ListChatQuery>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(expectedException);

        // Act & Assert
        var thrownException = await Should.ThrowAsync<InvalidOperationException>(async () =>
            await _endpoint.HandleAsync(query, _senderMock.Object)
        );

        thrownException.Message.ShouldBe("Database error");
        _senderMock.Verify(
            s => s.Send(It.IsAny<ListChatQuery>(), It.IsAny<CancellationToken>()),
            Times.Once
        );
    }

    [Test]
    public async Task GivenEmptyResultFromSender_WhenHandlingListChat_ThenShouldReturnOkWithEmptyList()
    {
        // Arrange
        var query = new ListChatQuery("NonExistent");
        var emptyResult = new List<ConversationDto>().AsReadOnly();

        _senderMock
            .Setup(s => s.Send(It.IsAny<ListChatQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(emptyResult);

        // Act
        var result = await _endpoint.HandleAsync(query, _senderMock.Object);

        // Assert
        result.ShouldBeOfType<Ok<IReadOnlyList<ConversationDto>>>();
        result.Value.ShouldBe(emptyResult);
        result.Value!.Count.ShouldBe(0);

        _senderMock.Verify(
            s => s.Send(It.IsAny<ListChatQuery>(), It.IsAny<CancellationToken>()),
            Times.Once
        );
    }

    [Test]
    public async Task GivenMultipleSequentialRequests_WhenHandlingListChat_ThenShouldHandleEachRequestIndependently()
    {
        // Arrange
        var query1 = new ListChatQuery("Chat1");
        var query2 = new ListChatQuery("Chat2", Guid.CreateVersion7(), true);
        var result1 = _conversationDtos.Take(1).ToList().AsReadOnly();
        var result2 = _conversationDtos.Skip(1).Take(2).ToList().AsReadOnly();

        _senderMock
            .SetupSequence(s => s.Send(It.IsAny<ListChatQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(result1)
            .ReturnsAsync(result2);

        // Act
        var response1 = await _endpoint.HandleAsync(query1, _senderMock.Object);
        var response2 = await _endpoint.HandleAsync(query2, _senderMock.Object);

        // Assert
        response1.ShouldBeOfType<Ok<IReadOnlyList<ConversationDto>>>();
        response1.Value.ShouldBe(result1);
        response1.Value!.Count.ShouldBe(1);

        response2.ShouldBeOfType<Ok<IReadOnlyList<ConversationDto>>>();
        response2.Value.ShouldBe(result2);
        response2.Value!.Count.ShouldBe(2);

        _senderMock.Verify(
            s => s.Send(It.IsAny<ListChatQuery>(), It.IsAny<CancellationToken>()),
            Times.Exactly(2)
        );
    }

    [Test]
    public async Task GivenQueryWithEmptyStringName_WhenHandlingListChat_ThenShouldPassEmptyStringToSender()
    {
        // Arrange
        var query = new ListChatQuery("");
        var expectedResult = _conversationDtos.AsReadOnly();

        _senderMock
            .Setup(s => s.Send(It.IsAny<ListChatQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedResult);

        // Act
        var result = await _endpoint.HandleAsync(query, _senderMock.Object);

        // Assert
        result.ShouldBeOfType<Ok<IReadOnlyList<ConversationDto>>>();
        _senderMock.Verify(
            s => s.Send(It.Is<ListChatQuery>(q => q.Name == ""), It.IsAny<CancellationToken>()),
            Times.Once
        );
    }

    [Test]
    public async Task GivenQueryWithEmptyGuidUserId_WhenHandlingListChat_ThenShouldPassEmptyGuidToSender()
    {
        // Arrange
        var emptyGuid = Guid.Empty;
        var query = new ListChatQuery(null, emptyGuid);
        var expectedResult = _conversationDtos.AsReadOnly();

        _senderMock
            .Setup(s => s.Send(It.IsAny<ListChatQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedResult);

        // Act
        var result = await _endpoint.HandleAsync(query, _senderMock.Object);

        // Assert
        result.ShouldBeOfType<Ok<IReadOnlyList<ConversationDto>>>();
        _senderMock.Verify(
            s =>
                s.Send(
                    It.Is<ListChatQuery>(q => q.UserId == emptyGuid),
                    It.IsAny<CancellationToken>()
                ),
            Times.Once
        );
    }

    [Test]
    public void GivenEndpointInstance_WhenCheckingImplementation_ThenShouldImplementIEndpointInterface()
    {
        // Arrange & Act & Assert
        _endpoint.ShouldNotBeNull();
        _endpoint.ShouldBeAssignableTo<
            IEndpoint<Ok<IReadOnlyList<ConversationDto>>, ListChatQuery, ISender>
        >();
    }

    private ConversationDto[] GenerateTestConversationDtos()
    {
        var conversations = _conversationFaker.Generate(5);
        return conversations
            .Select(c => new ConversationDto(
                c.Id,
                c.Name,
                c.UserId,
                new List<ConversationMessageDto>().AsReadOnly()
            ))
            .ToArray();
    }

    private IReadOnlyList<ConversationDto> GenerateConversationsWithMessages()
    {
        var conversations = _conversationFaker.Generate(3);
        return conversations
            .Select(c =>
            {
                var messages = _messageFaker
                    .Generate(3)
                    .Select(m => new ConversationMessageDto(
                        m.Id,
                        m.Text,
                        m.Role,
                        m.ParentMessageId,
                        m.CreatedAt
                    ))
                    .ToList()
                    .AsReadOnly();

                return new ConversationDto(c.Id, c.Name, c.UserId, messages);
            })
            .ToList()
            .AsReadOnly();
    }
}
