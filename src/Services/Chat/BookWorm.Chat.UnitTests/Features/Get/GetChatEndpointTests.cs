using BookWorm.Chassis.Endpoints;
using BookWorm.Chat.Features;
using BookWorm.Chat.Features.Get;
using MediatR;
using Microsoft.AspNetCore.Http.HttpResults;

namespace BookWorm.Chat.UnitTests.Features.Get;

public sealed class GetChatEndpointTests
{
    private readonly ConversationDto _conversationDto;
    private readonly Guid _conversationId;
    private readonly GetChatEndpoint _endpoint;
    private readonly Mock<ISender> _senderMock;

    public GetChatEndpointTests()
    {
        _senderMock = new();
        _endpoint = new();
        _conversationId = Guid.CreateVersion7();

        // Create a sample ConversationDto for testing
        _conversationDto = new(
            _conversationId,
            "Test Conversation",
            Guid.CreateVersion7(),
            new List<ConversationMessageDto>().AsReadOnly()
        );
    }

    [Test]
    public async Task GivenValidConversationId_WhenHandlingGetChat_ThenShouldReturnOkWithConversationDto()
    {
        // Arrange
        _senderMock
            .Setup(s => s.Send(It.IsAny<GetChatQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(_conversationDto);

        // Act
        var result = await _endpoint.HandleAsync(_conversationId, _senderMock.Object);

        // Assert
        result.ShouldBeOfType<Ok<ConversationDto>>();
        result.Value.ShouldBe(_conversationDto);
        result.Value?.Id.ShouldBe(_conversationId);
        result.Value?.Name.ShouldBe("Test Conversation");

        _senderMock.Verify(
            s =>
                s.Send(
                    It.Is<GetChatQuery>(q => q.Id == _conversationId),
                    It.IsAny<CancellationToken>()
                ),
            Times.Once
        );
    }

    [Test]
    public async Task GivenValidConversationIdWithCancellationToken_WhenHandlingGetChat_ThenShouldPassCancellationTokenToSender()
    {
        // Arrange
        using var cancellationTokenSource = new CancellationTokenSource();
        var cancellationToken = cancellationTokenSource.Token;

        _senderMock
            .Setup(s => s.Send(It.IsAny<GetChatQuery>(), cancellationToken))
            .ReturnsAsync(_conversationDto);

        // Act
        var result = await _endpoint.HandleAsync(
            _conversationId,
            _senderMock.Object,
            cancellationToken
        );

        // Assert
        result.ShouldBeOfType<Ok<ConversationDto>>();
        _senderMock.Verify(
            s => s.Send(It.Is<GetChatQuery>(q => q.Id == _conversationId), cancellationToken),
            Times.Once
        );
    }

    [Test]
    public async Task GivenEmptyGuid_WhenHandlingGetChat_ThenShouldSendQueryWithEmptyGuid()
    {
        // Arrange
        var emptyGuid = Guid.Empty;
        _senderMock
            .Setup(s => s.Send(It.IsAny<GetChatQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(_conversationDto);

        // Act
        var result = await _endpoint.HandleAsync(emptyGuid, _senderMock.Object);

        // Assert
        result.ShouldBeOfType<Ok<ConversationDto>>();
        _senderMock.Verify(
            s => s.Send(It.Is<GetChatQuery>(q => q.Id == emptyGuid), It.IsAny<CancellationToken>()),
            Times.Once
        );
    }

    [Test]
    public async Task GivenSenderThrowsException_WhenHandlingGetChat_ThenShouldPropagateException()
    {
        // Arrange
        var expectedException = new InvalidOperationException("Database error");
        _senderMock
            .Setup(s => s.Send(It.IsAny<GetChatQuery>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(expectedException);

        // Act
        var act = async () => await _endpoint.HandleAsync(_conversationId, _senderMock.Object);

        // Assert
        var exception = await act.ShouldThrowAsync<InvalidOperationException>();
        exception.Message.ShouldBe("Database error");

        _senderMock.Verify(
            s =>
                s.Send(
                    It.Is<GetChatQuery>(q => q.Id == _conversationId),
                    It.IsAny<CancellationToken>()
                ),
            Times.Once
        );
    }

    [Test]
    public async Task GivenMultipleSequentialRequests_WhenHandlingGetChat_ThenShouldHandleEachRequestIndependently()
    {
        // Arrange
        var conversationId1 = Guid.CreateVersion7();
        var conversationId2 = Guid.CreateVersion7();
        var conversationId3 = Guid.CreateVersion7();

        var conversationDto1 = _conversationDto with { Id = conversationId1, Name = "Chat 1" };
        var conversationDto2 = _conversationDto with { Id = conversationId2, Name = "Chat 2" };
        var conversationDto3 = _conversationDto with { Id = conversationId3, Name = "Chat 3" };

        _senderMock
            .Setup(s =>
                s.Send(
                    It.Is<GetChatQuery>(q => q.Id == conversationId1),
                    It.IsAny<CancellationToken>()
                )
            )
            .ReturnsAsync(conversationDto1);

        _senderMock
            .Setup(s =>
                s.Send(
                    It.Is<GetChatQuery>(q => q.Id == conversationId2),
                    It.IsAny<CancellationToken>()
                )
            )
            .ReturnsAsync(conversationDto2);

        _senderMock
            .Setup(s =>
                s.Send(
                    It.Is<GetChatQuery>(q => q.Id == conversationId3),
                    It.IsAny<CancellationToken>()
                )
            )
            .ReturnsAsync(conversationDto3);

        // Act
        var result1 = await _endpoint.HandleAsync(conversationId1, _senderMock.Object);
        var result2 = await _endpoint.HandleAsync(conversationId2, _senderMock.Object);
        var result3 = await _endpoint.HandleAsync(conversationId3, _senderMock.Object);

        // Assert
        result1.ShouldBeOfType<Ok<ConversationDto>>();
        result1.Value?.Id.ShouldBe(conversationId1);
        result1.Value?.Name.ShouldBe("Chat 1");

        result2.ShouldBeOfType<Ok<ConversationDto>>();
        result2.Value?.Id.ShouldBe(conversationId2);
        result2.Value?.Name.ShouldBe("Chat 2");

        result3.ShouldBeOfType<Ok<ConversationDto>>();
        result3.Value?.Id.ShouldBe(conversationId3);
        result3.Value?.Name.ShouldBe("Chat 3");

        _senderMock.Verify(
            s =>
                s.Send(
                    It.Is<GetChatQuery>(q => q.Id == conversationId1),
                    It.IsAny<CancellationToken>()
                ),
            Times.Once
        );
        _senderMock.Verify(
            s =>
                s.Send(
                    It.Is<GetChatQuery>(q => q.Id == conversationId2),
                    It.IsAny<CancellationToken>()
                ),
            Times.Once
        );
        _senderMock.Verify(
            s =>
                s.Send(
                    It.Is<GetChatQuery>(q => q.Id == conversationId3),
                    It.IsAny<CancellationToken>()
                ),
            Times.Once
        );
    }

    [Test]
    public async Task GivenValidRequest_WhenHandlingGetChat_ThenShouldCreateCorrectQueryObject()
    {
        // Arrange
        GetChatQuery? capturedQuery = null;
        _senderMock
            .Setup(s => s.Send(It.IsAny<GetChatQuery>(), It.IsAny<CancellationToken>()))
            .Callback<object, CancellationToken>(
                (query, _) => capturedQuery = query as GetChatQuery
            )
            .ReturnsAsync(_conversationDto);

        // Act
        await _endpoint.HandleAsync(_conversationId, _senderMock.Object);

        // Assert
        capturedQuery.ShouldNotBeNull();
        capturedQuery.Id.ShouldBe(_conversationId);
    }

    [Test]
    public async Task GivenConversationWithMessages_WhenHandlingGetChat_ThenShouldReturnCompleteConversationDto()
    {
        // Arrange
        var messages = new List<ConversationMessageDto>
        {
            new(Guid.CreateVersion7(), "Hello", "user", null, DateTime.UtcNow),
            new(Guid.CreateVersion7(), "Hi there!", "assistant", null, DateTime.UtcNow),
        }.AsReadOnly();

        var conversationWithMessages = _conversationDto with { Messages = messages };

        _senderMock
            .Setup(s => s.Send(It.IsAny<GetChatQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(conversationWithMessages);

        // Act
        var result = await _endpoint.HandleAsync(_conversationId, _senderMock.Object); // Assert
        result.ShouldBeOfType<Ok<ConversationDto>>();
        result.Value.ShouldNotBeNull();
        result.Value.Messages.Count.ShouldBe(2);
        result.Value.Messages[0].Text.ShouldBe("Hello");
        result.Value.Messages[1].Text.ShouldBe("Hi there!");
    }

    [Test]
    public void GivenGetChatEndpoint_WhenCheckingImplementedInterfaces_ThenShouldImplementCorrectInterface()
    {
        // Arrange & Act
        var endpoint = new GetChatEndpoint();

        // Assert
        endpoint.ShouldBeAssignableTo<IEndpoint<Ok<ConversationDto>, Guid, ISender>>();
    }

    [Test]
    public void GivenConversationId_WhenCreatingGetEndpointQuery_ThenPropertiesShouldBeCorrectlyInitialized()
    {
        // Arrange & Act
        var query = new GetChatQuery(_conversationId);

        // Assert
        query.Id.ShouldBe(_conversationId);
    }

    [Test]
    public void GivenTwoGetEndpointQueriesWithSameId_WhenComparing_ThenShouldBeEqual()
    {
        // Arrange
        var query1 = new GetChatQuery(_conversationId);
        var query2 = new GetChatQuery(_conversationId);

        // Act & Assert
        query1.ShouldBe(query2);
        query1.GetHashCode().ShouldBe(query2.GetHashCode());
    }

    [Test]
    public void GivenTwoGetEndpointQueriesWithDifferentIds_WhenComparing_ThenShouldNotBeEqual()
    {
        // Arrange
        var conversationId1 = Guid.CreateVersion7();
        var conversationId2 = Guid.CreateVersion7();
        var query1 = new GetChatQuery(conversationId1);
        var query2 = new GetChatQuery(conversationId2);

        // Act & Assert
        query1.ShouldNotBe(query2);
        query1.GetHashCode().ShouldNotBe(query2.GetHashCode());
    }
}
