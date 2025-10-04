using BookWorm.Chassis.CQRS.Query;
using BookWorm.Chat.Features.Visualizer;
using BookWorm.Chat.Infrastructure.AgentOrchestration;
using BookWorm.SharedKernel;

namespace BookWorm.Chat.UnitTests.Features.Visualizer;

public sealed class VisualizerWorkflowQueryTests
{
    private readonly Mock<IAgentOrchestrationService> _agentOrchestrationServiceMock = new();
    private readonly VisualizerWorkflowHandler _handler;

    public VisualizerWorkflowQueryTests()
    {
        _handler = new(_agentOrchestrationServiceMock.Object);
    }

    [Test]
    public void GivenVisualizerWorkflowQuery_WhenCreating_ThenShouldBeOfCorrectType()
    {
        // Arrange & Act
        var query = new VisualizerWorkflowQuery();

        // Assert
        query.ShouldNotBeNull();
        query.ShouldBeOfType<VisualizerWorkflowQuery>();
        query.ShouldBeAssignableTo<IQuery<string>>();
        query.Type.ShouldBe(VisualizationType.Mermaid);
    }

    [Test]
    public void GivenVisualizerWorkflowQueryWithType_WhenCreating_ThenShouldHaveCorrectType()
    {
        // Arrange & Act
        var query = new VisualizerWorkflowQuery(VisualizationType.Dot);

        // Assert
        query.ShouldNotBeNull();
        query.Type.ShouldBe(VisualizationType.Dot);
    }

    [Test]
    public void GivenTwoVisualizerWorkflowQueriesWithSameValues_WhenComparing_ThenShouldBeEqual()
    {
        // Arrange
        var query1 = new VisualizerWorkflowQuery();
        var query2 = new VisualizerWorkflowQuery();

        // Act & Assert
        query1.ShouldBe(query2);
        query1.GetHashCode().ShouldBe(query2.GetHashCode());
        (query1 == query2).ShouldBeTrue();
        (query1 != query2).ShouldBeFalse();
    }

    [Test]
    public void GivenTwoVisualizerWorkflowQueriesWithDifferentTypes_WhenComparing_ThenShouldNotBeEqual()
    {
        // Arrange
        var query1 = new VisualizerWorkflowQuery();
        var query2 = new VisualizerWorkflowQuery(VisualizationType.Dot);

        // Act & Assert
        query1.ShouldNotBe(query2);
        (query1 == query2).ShouldBeFalse();
        (query1 != query2).ShouldBeTrue();
    }

    [Test]
    public void GivenMermaidVisualizationType_WhenHandling_ThenShouldCallBuildAgentsWorkflow()
    {
        // Arrange
        var query = new VisualizerWorkflowQuery();

        // Act & Assert - Handler will call BuildAgentsWorkflow which will fail since mock returns null
        // This tests that the service dependency is correctly called
        Should.Throw<NullReferenceException>(async () =>
            await _handler.Handle(query, CancellationToken.None)
        );

        _agentOrchestrationServiceMock.Verify(x => x.BuildAgentsWorkflow(), Times.Once);
    }

    [Test]
    public void GivenDefaultQuery_WhenCreating_ThenShouldHaveMermaidTypeAsDefault()
    {
        // Arrange & Act
        var query = new VisualizerWorkflowQuery();

        // Assert
        query.Type.ShouldBe(VisualizationType.Mermaid);
    }

    [Test]
    public void GivenCancellationToken_WhenHandling_ThenShouldAcceptCancellationToken()
    {
        // Arrange
        var query = new VisualizerWorkflowQuery();
        var cts = new CancellationTokenSource();

        // Act & Assert
        Should.Throw<NullReferenceException>(async () => await _handler.Handle(query, cts.Token));

        _agentOrchestrationServiceMock.Verify(x => x.BuildAgentsWorkflow(), Times.Once);
    }

    [Test]
    public void GivenHandler_WhenCreated_ThenShouldBeOfCorrectType()
    {
        // Assert
        _handler.ShouldNotBeNull();
        _handler.ShouldBeOfType<VisualizerWorkflowHandler>();
        _handler.ShouldBeAssignableTo<IQueryHandler<VisualizerWorkflowQuery, string>>();
    }
}
