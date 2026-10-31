using BookWorm.Chat.Memory;
using BookWorm.Chat.Memory.Extraction;
using BookWorm.Chat.Memory.Semantic;
using Microsoft.Agents.AI;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.Logging.Abstractions;

namespace BookWorm.Chat.UnitTests.Memory;

public sealed class UserMemoryContextProviderTests
{
    private const string UserA = "user-a";
    private const string UserB = "user-b";

    private readonly AIAgent _agent = new Mock<AIAgent>().Object;
    private readonly AgentSession _session = new Mock<AgentSession>().Object;
    private readonly Mock<IUserIdentityAccessor> _identityAccessorMock = new();
    private readonly Mock<IUserMemoryService> _memoryServiceMock = new();
    private readonly Mock<IMemoryExtractionQueue> _queueMock = new();
    private readonly UserMemoryContextProvider _sut;

    public UserMemoryContextProviderTests()
    {
        _queueMock.Setup(q => q.TryEnqueue(It.IsAny<MemoryExtractionRequest>())).Returns(true);

        _sut = new(
            _identityAccessorMock.Object,
            _memoryServiceMock.Object,
            _queueMock.Object,
            NullLogger<UserMemoryContextProvider>.Instance
        );
    }

    [Test]
    public async Task GivenAnonymousUser_WhenInvoking_ThenShouldNotRecallOrInjectMemory()
    {
        _identityAccessorMock.Setup(a => a.GetUserId()).Returns((string?)null);

        var result = await InvokeAsync("Recommend a book");

        result.Instructions.ShouldBeNull();
        _memoryServiceMock.Verify(
            s =>
                s.RecallAsync(
                    It.IsAny<string>(),
                    It.IsAny<string?>(),
                    It.IsAny<CancellationToken>()
                ),
            Times.Never
        );
    }

    [Test]
    public async Task GivenUserWithoutMemories_WhenInvoking_ThenShouldNotInjectAnything()
    {
        _identityAccessorMock.Setup(a => a.GetUserId()).Returns(UserA);
        _memoryServiceMock
            .Setup(s => s.RecallAsync(UserA, It.IsAny<string?>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(UserMemorySnapshot.Empty);

        var result = await InvokeAsync("Recommend a book");

        result.Instructions.ShouldBeNull();
        result.Messages.ShouldNotBeNull();
        result.Messages.Count().ShouldBe(1);
    }

    [Test]
    public async Task GivenUserWithMemories_WhenInvoking_ThenShouldRecallUsingLatestUserMessage()
    {
        _identityAccessorMock.Setup(a => a.GetUserId()).Returns(UserA);
        _memoryServiceMock
            .Setup(s => s.RecallAsync(UserA, It.IsAny<string?>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(CreateSnapshot("Science Fiction"));

        var result = await InvokeAsync("Any new space opera?");

        result.Instructions.ShouldNotBeNull();
        result.Instructions.ShouldContain(MemoryPromptFormatter.OpenTag);
        result.Instructions.ShouldContain("Science Fiction");
        _memoryServiceMock.Verify(
            s => s.RecallAsync(UserA, "Any new space opera?", It.IsAny<CancellationToken>()),
            Times.Once
        );
    }

    [Test]
    public async Task GivenTwoUsers_WhenInvoking_ThenEachShouldOnlySeeTheirOwnMemories()
    {
        _memoryServiceMock
            .Setup(s => s.RecallAsync(UserA, It.IsAny<string?>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(CreateSnapshot("Science Fiction"));
        _memoryServiceMock
            .Setup(s => s.RecallAsync(UserB, It.IsAny<string?>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(CreateSnapshot("Romance"));

        _identityAccessorMock.Setup(a => a.GetUserId()).Returns(UserA);
        var resultA = await InvokeAsync("Recommend a book");

        _identityAccessorMock.Setup(a => a.GetUserId()).Returns(UserB);
        var resultB = await InvokeAsync("Recommend a book");

        resultA.Instructions.ShouldNotBeNull();
        resultA.Instructions.ShouldContain("Science Fiction");
        resultA.Instructions.ShouldNotContain("Romance");

        resultB.Instructions.ShouldNotBeNull();
        resultB.Instructions.ShouldContain("Romance");
        resultB.Instructions.ShouldNotContain("Science Fiction");
    }

    [Test]
    public async Task GivenAuthenticatedUser_WhenInvoked_ThenShouldEnqueueLatestExchangeForThatUser()
    {
        _identityAccessorMock.Setup(a => a.GetUserId()).Returns(UserA);

        await _sut.InvokedAsync(
            new(
                _agent,
                _session,
                [
                    new(ChatRole.User, "I loved Dune"),
                    new(ChatRole.Assistant, "Great choice"),
                    new(ChatRole.User, "I prefer science fiction"),
                ],
                [new(ChatRole.Assistant, "Here are some science fiction books")]
            )
        );

        _queueMock.Verify(
            q =>
                q.TryEnqueue(
                    It.Is<MemoryExtractionRequest>(r =>
                        r.UserId == UserA
                        && r.UserMessage == "I prefer science fiction"
                        && r.AssistantResponse == "Here are some science fiction books"
                    )
                ),
            Times.Once
        );
    }

    [Test]
    public async Task GivenAnonymousUser_WhenInvoked_ThenShouldNotEnqueueExchange()
    {
        _identityAccessorMock.Setup(a => a.GetUserId()).Returns((string?)null);

        await _sut.InvokedAsync(
            new(
                _agent,
                _session,
                [new(ChatRole.User, "I prefer science fiction")],
                [new(ChatRole.Assistant, "Noted")]
            )
        );

        _queueMock.Verify(q => q.TryEnqueue(It.IsAny<MemoryExtractionRequest>()), Times.Never);
    }

    [Test]
    public async Task GivenFailedInvocation_WhenInvoked_ThenShouldNotEnqueueExchange()
    {
        _identityAccessorMock.Setup(a => a.GetUserId()).Returns(UserA);

        await _sut.InvokedAsync(
            new(
                _agent,
                _session,
                [new(ChatRole.User, "I prefer science fiction")],
                new InvalidOperationException("boom")
            )
        );

        _queueMock.Verify(q => q.TryEnqueue(It.IsAny<MemoryExtractionRequest>()), Times.Never);
    }

    private async Task<AIContext> InvokeAsync(string userMessage)
    {
        return await _sut.InvokingAsync(
            new(_agent, _session, new() { Messages = [new(ChatRole.User, userMessage)] })
        );
    }

    private static UserMemorySnapshot CreateSnapshot(string genre)
    {
        return new(
            [
                new(
                    Guid.CreateVersion7(),
                    PreferenceKind.Genre,
                    genre,
                    PreferenceSentiment.Like,
                    0.8,
                    2,
                    DateTime.UtcNow
                ),
            ],
            []
        );
    }
}
