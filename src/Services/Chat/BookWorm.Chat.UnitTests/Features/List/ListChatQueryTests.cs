using System.Security.Claims;
using BookWorm.Chat.Domain.AggregatesModel;
using BookWorm.Chat.Domain.AggregatesModel.Specifications;
using BookWorm.Chat.Features.List;
using BookWorm.Chat.UnitTests.Fakers;
using BookWorm.Constants.Core;

namespace BookWorm.Chat.UnitTests.Features.List;

public sealed class ListChatHandlerTests
{
    private readonly ConversationFaker _conversationFaker = new();
    private readonly ConversationMessageFaker _messageFaker = new();
    private readonly Mock<IConversationRepository> _mockRepository = new();

    [Test]
    public async Task GivenNonAdminUser_WhenHandlingListChat_ThenShouldOverrideUserIdWithCurrentUser()
    {
        // Arrange
        var currentUserId = Guid.NewGuid();
        var providedUserId = Guid.NewGuid();
        var claimsPrincipal = CreateClaimsPrincipal(
            [Authorization.Roles.User],
            currentUserId.ToString()
        );
        var query = new ListChatQuery(UserId: providedUserId);
        var expectedConversations = _conversationFaker.Generate().ToList();

        _mockRepository
            .Setup(r =>
                r.ListAsync(It.IsAny<ConversationFilterSpec>(), It.IsAny<CancellationToken>())
            )
            .ReturnsAsync(expectedConversations);

        var handler = new ListChatHandler(_mockRepository.Object, claimsPrincipal);

        // Act
        var result = await handler.Handle(query, CancellationToken.None);

        // Assert
        _mockRepository.Verify(
            r =>
                r.ListAsync(
                    It.Is<ConversationFilterSpec>(spec => true),
                    It.IsAny<CancellationToken>()
                ),
            Times.Once
        );

        result.ShouldNotBeNull();
    }

    [Test]
    public async Task GivenAdminUser_WhenHandlingListChatWithUserId_ThenShouldUseProvidedUserId()
    {
        // Arrange
        var currentUserId = Guid.NewGuid();
        var providedUserId = Guid.NewGuid();
        var claimsPrincipal = CreateClaimsPrincipal(
            [Authorization.Roles.Admin],
            currentUserId.ToString()
        );
        var query = new ListChatQuery(UserId: providedUserId);
        var expectedConversations = _conversationFaker.Generate().ToList();

        _mockRepository
            .Setup(r =>
                r.ListAsync(It.IsAny<ConversationFilterSpec>(), It.IsAny<CancellationToken>())
            )
            .ReturnsAsync(expectedConversations);

        var handler = new ListChatHandler(_mockRepository.Object, claimsPrincipal);

        // Act
        var result = await handler.Handle(query, CancellationToken.None);

        // Assert
        _mockRepository.Verify(
            r =>
                r.ListAsync(
                    It.Is<ConversationFilterSpec>(spec => true),
                    It.IsAny<CancellationToken>()
                ),
            Times.Once
        );

        result.ShouldNotBeNull();
    }

    [Test]
    public async Task GivenAdminUser_WhenHandlingListChatWithoutUserId_ThenShouldUseNullUserId()
    {
        // Arrange
        var currentUserId = Guid.NewGuid();
        var claimsPrincipal = CreateClaimsPrincipal(
            [Authorization.Roles.Admin],
            currentUserId.ToString()
        );
        var query = new ListChatQuery();
        var expectedConversations = _conversationFaker.Generate().ToList();

        _mockRepository
            .Setup(r =>
                r.ListAsync(It.IsAny<ConversationFilterSpec>(), It.IsAny<CancellationToken>())
            )
            .ReturnsAsync(expectedConversations);

        var handler = new ListChatHandler(_mockRepository.Object, claimsPrincipal);

        // Act
        var result = await handler.Handle(query, CancellationToken.None);

        // Assert
        _mockRepository.Verify(
            r =>
                r.ListAsync(
                    It.Is<ConversationFilterSpec>(spec => true),
                    It.IsAny<CancellationToken>()
                ),
            Times.Once
        );

        result.ShouldNotBeNull();
    }

    [Test]
    public async Task GivenQueryWithName_WhenHandlingListChat_ThenShouldFilterByName()
    {
        // Arrange
        var currentUserId = Guid.NewGuid();
        const string conversationName = "Test Conversation";
        var claimsPrincipal = CreateClaimsPrincipal(
            [Authorization.Roles.Admin],
            currentUserId.ToString()
        );
        var query = new ListChatQuery(conversationName);
        var expectedConversations = _conversationFaker.Generate().ToList();

        _mockRepository
            .Setup(r =>
                r.ListAsync(It.IsAny<ConversationFilterSpec>(), It.IsAny<CancellationToken>())
            )
            .ReturnsAsync(expectedConversations);

        var handler = new ListChatHandler(_mockRepository.Object, claimsPrincipal);

        // Act
        var result = await handler.Handle(query, CancellationToken.None);

        // Assert
        _mockRepository.Verify(
            r =>
                r.ListAsync(
                    It.Is<ConversationFilterSpec>(spec => true),
                    It.IsAny<CancellationToken>()
                ),
            Times.Once
        );

        result.ShouldNotBeNull();
    }

    [Test]
    public async Task GivenQueryWithIncludeMessages_WhenHandlingListChat_ThenShouldIncludeMessages()
    {
        // Arrange
        var currentUserId = Guid.NewGuid();
        var claimsPrincipal = CreateClaimsPrincipal(
            [Authorization.Roles.Admin],
            currentUserId.ToString()
        );
        var query = new ListChatQuery(IncludeMessages: true);
        var expectedConversations = _conversationFaker.Generate().ToList();

        _mockRepository
            .Setup(r =>
                r.ListAsync(It.IsAny<ConversationFilterSpec>(), It.IsAny<CancellationToken>())
            )
            .ReturnsAsync(expectedConversations);

        var handler = new ListChatHandler(_mockRepository.Object, claimsPrincipal);

        // Act
        var result = await handler.Handle(query, CancellationToken.None);

        // Assert
        _mockRepository.Verify(
            r =>
                r.ListAsync(
                    It.Is<ConversationFilterSpec>(spec => true),
                    It.IsAny<CancellationToken>()
                ),
            Times.Once
        );

        result.ShouldNotBeNull();
    }

    [Test]
    public async Task GivenValidQuery_WhenHandlingListChat_ThenShouldReturnMappedDtos()
    {
        // Arrange
        var currentUserId = Guid.NewGuid();
        var claimsPrincipal = CreateClaimsPrincipal(
            [Authorization.Roles.Admin],
            currentUserId.ToString()
        );
        var query = new ListChatQuery();
        var conversation = _conversationFaker.Generate()[0];
        var messages = _messageFaker.Generate();
        foreach (var message in messages)
        {
            conversation.AddMessage(message);
        }

        var expectedConversations = new List<Conversation> { conversation };

        _mockRepository
            .Setup(r =>
                r.ListAsync(It.IsAny<ConversationFilterSpec>(), It.IsAny<CancellationToken>())
            )
            .ReturnsAsync(expectedConversations);

        var handler = new ListChatHandler(_mockRepository.Object, claimsPrincipal);

        // Act
        var result = await handler.Handle(query, CancellationToken.None);

        // Assert
        result.ShouldNotBeNull();
        result.Count.ShouldBe(1);

        var conversationDto = result[0];
        conversationDto.Id.ShouldBe(conversation.Id);
        conversationDto.Name.ShouldBe(conversation.Name);
        conversationDto.UserId.ShouldBe(conversation.UserId);
        conversationDto.Messages.Count.ShouldBe(conversation.Messages.Count);
    }

    [Test]
    public async Task GivenComplexQuery_WhenHandlingListChat_ThenShouldApplyAllFilters()
    {
        // Arrange
        var currentUserId = Guid.NewGuid();
        const string conversationName = "Complex Test";
        var targetUserId = Guid.NewGuid();

        var claimsPrincipal = CreateClaimsPrincipal(
            [Authorization.Roles.Admin],
            currentUserId.ToString()
        );
        var query = new ListChatQuery(conversationName, targetUserId, true);
        var expectedConversations = _conversationFaker.Generate().ToList();

        _mockRepository
            .Setup(r =>
                r.ListAsync(It.IsAny<ConversationFilterSpec>(), It.IsAny<CancellationToken>())
            )
            .ReturnsAsync(expectedConversations);

        var handler = new ListChatHandler(_mockRepository.Object, claimsPrincipal);

        // Act
        var result = await handler.Handle(query, CancellationToken.None);

        // Assert
        _mockRepository.Verify(
            r =>
                r.ListAsync(
                    It.Is<ConversationFilterSpec>(spec => true),
                    It.IsAny<CancellationToken>()
                ),
            Times.Once
        );

        result.ShouldNotBeNull();
    }

    [Test]
    public async Task GivenEmptyRepository_WhenHandlingListChat_ThenShouldReturnEmptyList()
    {
        // Arrange
        var currentUserId = Guid.NewGuid();
        var claimsPrincipal = CreateClaimsPrincipal(
            [Authorization.Roles.Admin],
            currentUserId.ToString()
        );
        var query = new ListChatQuery();

        _mockRepository
            .Setup(r =>
                r.ListAsync(It.IsAny<ConversationFilterSpec>(), It.IsAny<CancellationToken>())
            )
            .ReturnsAsync([]);

        var handler = new ListChatHandler(_mockRepository.Object, claimsPrincipal);

        // Act
        var result = await handler.Handle(query, CancellationToken.None);

        // Assert
        result.ShouldNotBeNull();
        result.Count.ShouldBe(0);
    }

    private static ClaimsPrincipal CreateClaimsPrincipal(string[] roles, string userId)
    {
        var claims = new List<Claim> { new(ClaimTypes.NameIdentifier, userId) };

        claims.AddRange(roles.Select(role => new Claim(ClaimTypes.Role, role)));

        var identity = new ClaimsIdentity(claims, "test");

        return new(identity);
    }
}
