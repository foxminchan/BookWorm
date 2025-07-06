﻿using System.Diagnostics.CodeAnalysis;
using BookWorm.Chassis.AI;
using Microsoft.Extensions.ServiceDiscovery;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.Agents;
using Microsoft.SemanticKernel.Connectors.Ollama;

namespace BookWorm.Chat.Agents;

[ExcludeFromCodeCoverage]
public static class BookAgent
{
    private const string Name = nameof(BookAgent);

    private const string Description =
        "An agent that searches for books, provides relevant information, and offers personalized recommendations based on user preferences and behavior.";

    private const string Instructions = """
        You are an AI assistant for BookWorm bookstore that provides comprehensive book assistance including:

        **Search Capabilities:**
        - Search the BookWorm catalog for books based on user queries
        - Provide accurate book information from the catalog
        - Use the SearchCatalog function to find books that match requests
        - Only include details about books present in the catalog

        **Recommendation Features:**
        - Personalized book recommendations based on user preferences
        - Suggestions based on reading history and ratings
        - Similar books and related genres
        - Trending and popular books recommendations
        - Recommendations for specific occasions or gifts

        **Interaction Style:**
        - Ask relevant questions to better understand user preferences when providing recommendations
        - Provide helpful suggestions based on user responses
        - Always provide accurate information based on search results
        - Be friendly and knowledgeable about books and reading

        Whether users are searching for specific books or looking for recommendations, help them discover their next great read!
        """;

    public static async Task<ChatCompletionAgent> CreateAgentWithPluginsAsync(
        Kernel kernel,
        IMcpClient mcpClient,
        ServiceEndpointResolver resolver
    )
    {
        var agentKernel = kernel.Clone();

        var toolsTask = mcpClient.ListToolsAsync().AsTask();
        var ratingAgentTask = resolver.ConnectRemoteAgent(
            $"{Protocol.HttpOrHttps}://{Application.Rating}/agents/rating"
        );
        await Task.WhenAll(toolsTask, ratingAgentTask);

        agentKernel.Plugins.AddFromFunctions(
            nameof(BookWorm),
            (await toolsTask).Select(aiFunction => aiFunction.AsKernelFunction())
        );

        agentKernel.Plugins.Add(await ratingAgentTask);

        return new()
        {
            Instructions = Instructions,
            Name = Name,
            Description = Description,
            Kernel = agentKernel,
            Arguments = new(
                new OllamaPromptExecutionSettings
                {
                    Temperature = 0.3f,
                    FunctionChoiceBehavior = FunctionChoiceBehavior.Auto(
                        options: new() { RetainArgumentTypes = true }
                    ),
                }
            ),
        };
    }

    private static async Task<KernelPlugin> ConnectRemoteAgent(
        this ServiceEndpointResolver resolver,
        string agentUri
    )
    {
        var resolvedUrl = await resolver.ResolveServiceEndpointUrl(agentUri, "/");
        var agent = await resolvedUrl.CreateAgentAsync();
        var agentFunction = AgentKernelFunctionFactory.CreateFromAgent(agent);
        return KernelPluginFactory.CreateFromFunctions("AgentPlugin", [agentFunction]);
    }
}
