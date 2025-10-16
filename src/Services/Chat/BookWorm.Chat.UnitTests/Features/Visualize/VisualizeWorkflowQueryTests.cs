using BookWorm.Chat.Features.Visualize;
using BookWorm.Chat.Infrastructure.AgentOrchestration;
using BookWorm.SharedKernel;
using Mediator;

namespace BookWorm.Chat.UnitTests.Features.Visualize;

public sealed class VisualizeWorkflowQueryTests
{
    private readonly Mock<IAgentOrchestrationService> _agentOrchestrationServiceMock = new();
    private readonly VisualizerWorkflowHandler _handler;

    public VisualizeWorkflowQueryTests()
    {
        _handler = new(_agentOrchestrationServiceMock.Object);
    }

    [Test]
    public void GivenVisualizerWorkflowQuery_WhenCreating_ThenShouldBeOfCorrectType()
    {
        // Arrange & Act
        var query = new VisualizeWorkflowQuery();

        // Assert
        query.ShouldNotBeNull();
        query.ShouldBeOfType<VisualizeWorkflowQuery>();
        query.ShouldBeAssignableTo<IQuery<string>>();
        query.Type.ShouldBe(Visualizations.Mermaid);
    }

    [Test]
    public void GivenVisualizerWorkflowQueryWithType_WhenCreating_ThenShouldHaveCorrectType()
    {
        // Arrange & Act
        var query = new VisualizeWorkflowQuery(Visualizations.Dot);

        // Assert
        query.ShouldNotBeNull();
        query.Type.ShouldBe(Visualizations.Dot);
    }

    [Test]
    public void GivenTwoVisualizerWorkflowQueriesWithSameValues_WhenComparing_ThenShouldBeEqual()
    {
        // Arrange
        var query1 = new VisualizeWorkflowQuery();
        var query2 = new VisualizeWorkflowQuery();

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
        var query1 = new VisualizeWorkflowQuery();
        var query2 = new VisualizeWorkflowQuery(Visualizations.Dot);

        // Act & Assert
        query1.ShouldNotBe(query2);
        (query1 == query2).ShouldBeFalse();
        (query1 != query2).ShouldBeTrue();
    }

    [Test]
    public void GivenMermaidVisualizationType_WhenHandling_ThenShouldCallBuildAgentsWorkflow()
    {
        // Arrange
        var query = new VisualizeWorkflowQuery();

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
        var query = new VisualizeWorkflowQuery();

        // Assert
        query.Type.ShouldBe(Visualizations.Mermaid);
    }

    [Test]
    public void GivenCancellationToken_WhenHandling_ThenShouldAcceptCancellationToken()
    {
        // Arrange
        var query = new VisualizeWorkflowQuery();
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
        _handler.ShouldBeAssignableTo<IQueryHandler<VisualizeWorkflowQuery, string>>();
    }
}
