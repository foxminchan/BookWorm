using BookWorm.Chassis.AI.Agents;
using BookWorm.Chassis.AI.Governance;
using Microsoft.Agents.AI;
using Microsoft.Extensions.AI;
using ModelContextProtocol.Client;

namespace BookWorm.Chassis.AI.Extensions;

public static class ChatClientAgentExtensions
{
    extension(IChatClient chatClient)
    {
        /// <summary>
        ///     Initializes a governed subagent from a YAML definition and its declared MCP tools.
        /// </summary>
        /// <param name="services">The service provider used to resolve governance services.</param>
        /// <param name="mcpClient">The MCP client that provides the tools declared by the definition.</param>
        /// <param name="definitionFile">The definition path relative to the application output directory.</param>
        /// <param name="agentName">The expected agent name in the definition.</param>
        /// <returns>The initialized subagent.</returns>
        public AIAgent InitSubAgent(
            IServiceProvider services,
            McpClient mcpClient,
            string definitionFile,
            string agentName
        )
        {
            ArgumentNullException.ThrowIfNull(chatClient);
            ArgumentNullException.ThrowIfNull(services);
            ArgumentNullException.ThrowIfNull(mcpClient);
            ArgumentException.ThrowIfNullOrWhiteSpace(definitionFile);
            ArgumentException.ThrowIfNullOrWhiteSpace(agentName);

            var (options, toolNames) = AgentDefinitionLoader.Load(definitionFile, agentName);
            options.ChatOptions?.Tools = mcpClient.GetAgentTools(toolNames);
            options.AllowConcurrentInvocation = true;

            var agent = new ChatClientAgent(chatClient, options);

            return agent.WithBookWormGovernance(services, agentName);
        }
    }
}
