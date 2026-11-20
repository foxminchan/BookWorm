using AgentGovernance;
using AgentGovernance.Policy;
using BookWorm.Chassis.AI.Agents;

namespace BookWorm.Chat.UnitTests.Orchestration;

public sealed class AgentDefinitionAndGovernanceTests
{
    [Test]
    [Arguments("Agents/BookSearch/book-agent.yaml", Constants.Other.Agents.BookAgent)]
    [Arguments("Agents/CustomerSupport/qa-agent.yaml", Constants.Other.Agents.QAAgent)]
    [Arguments("Agents/BookSearch/catalog-researcher.yaml", "book-catalog-researcher")]
    [Arguments(
        "Agents/BookSearch/recommendation-researcher.yaml",
        "book-recommendation-researcher"
    )]
    public void GivenYamlAgentDefinition_WhenLoading_ThenMatchesExpectedRegistrationKey(
        string definitionFile,
        string expectedName
    )
    {
        // Act
        var (options, _) = AgentDefinitionLoader.Load(definitionFile, expectedName);

        // Assert
        options.Name.ShouldBe(expectedName);
        options.Description.ShouldNotBeNullOrWhiteSpace();
        options.ChatOptions?.Instructions.ShouldNotBeNullOrWhiteSpace();
    }

    [Test]
    [MethodDataSource(nameof(PermittedActions))]
    public void GivenChatGovernancePolicies_WhenEvaluatingPermittedActions_ThenShouldAllow(
        string agentDid,
        string toolName
    )
    {
        // Arrange
        using var kernel = CreateChatGovernanceKernel();

        // Act
        var decision = kernel.EvaluateToolCall(agentDid, toolName);

        // Assert
        decision.Allowed.ShouldBeTrue(decision.Reason);
    }

    [Test]
    [Arguments("did:mesh:bookworm:qa", "search_catalog")]
    [Arguments("did:mesh:bookworm:qa", "addToBasket")]
    [Arguments("did:mesh:bookworm:book", "execute_shell")]
    [Arguments(
        "did:mesh:bookworm:book-catalog-researcher",
        "delegate_book_recommendation_researcher"
    )]
    public void GivenChatGovernancePolicies_WhenEvaluatingUnauthorizedTools_ThenShouldDeny(
        string agentDid,
        string toolName
    )
    {
        // Arrange
        using var kernel = CreateChatGovernanceKernel();

        // Act
        var decision = kernel.EvaluateToolCall(agentDid, toolName);

        // Assert
        decision.Allowed.ShouldBeFalse(decision.Reason);
    }

    public static IEnumerable<(string AgentDid, string ToolName)> PermittedActions()
    {
        (string AgentDid, string[] Tools)[] cases =
        [
            (
                "did:mesh:bookworm:book",
                [
                    "agent_run",
                    "agent_run_streaming",
                    "search_catalog",
                    "get_book",
                    "load_skill",
                    "read_skill_resource",
                    "mode_get",
                    "mode_set",
                    "todos_add",
                    "file_memory_ls",
                    "handoff_to_rating",
                    "handoff_to_qa",
                    "showBookCard",
                    "showBookList",
                    "addToBasket",
                    "delegate_book_catalog_researcher",
                    "delegate_book_recommendation_researcher",
                ]
            ),
            (
                "did:mesh:bookworm:qa",
                [
                    "agent_run",
                    "agent_run_streaming",
                    "load_skill",
                    "read_skill_resource",
                    "handoff_to_book",
                ]
            ),
            ("did:mesh:bookworm:book-catalog-researcher", ["agent_run", "search_catalog"]),
            ("did:mesh:bookworm:book-recommendation-researcher", ["agent_run", "search_catalog"]),
        ];

        foreach (var (agentDid, tools) in cases)
        {
            foreach (var tool in tools)
            {
                yield return (agentDid, tool);
            }
        }
    }

    private static GovernanceKernel CreateChatGovernanceKernel()
    {
        return new(
            new()
            {
                PolicyPaths =
                [
                    Path.Combine(
                        AppContext.BaseDirectory,
                        "AI",
                        "Governance",
                        "Policies",
                        "default.yaml"
                    ),
                    Path.Combine(AppContext.BaseDirectory, "Policies", "chat-agents.yaml"),
                ],
                ConflictStrategy = ConflictResolutionStrategy.DenyOverrides,
                EnableRings = true,
                EnablePromptInjectionDetection = true,
                EnableCircuitBreaker = true,
            }
        );
    }
}
