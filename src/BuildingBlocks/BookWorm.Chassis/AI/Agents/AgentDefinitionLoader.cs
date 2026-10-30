using System.IO;
using System.Linq;
using Microsoft.Agents.AI;
using Microsoft.Agents.ObjectModel;
using Microsoft.Agents.ObjectModel.Yaml;
using Microsoft.PowerFx;

namespace BookWorm.Chassis.AI.Agents;

public static class AgentDefinitionLoader
{
    /// <summary>
    /// Loads a prompt agent definition from the application output directory.
    /// </summary>
    /// <param name="definitionFile">The path to the definition, relative to the application output directory.</param>
    /// <param name="expectedName">The agent name expected by the registering service.</param>
    /// <returns>The configured options and declared tool names.</returns>
    /// <exception cref="InvalidDataException">
    /// The definition is not a prompt agent, its name does not match, or required metadata is missing.
    /// </exception>
    public static (
        ChatClientAgentOptions Options,
        IReadOnlySet<string> ToolNames
    ) Load(string definitionFile, string expectedName)
    {
        var path = Path.Combine(AppContext.BaseDirectory, definitionFile);
        using var reader = File.OpenText(path);

        if (YamlSerializer.Deserialize<BotElement>(reader) is not GptComponentMetadata definition)
        {
            throw new InvalidDataException(
                $"Agent definition '{path}' must contain a prompt agent."
            );
        }

        if (!string.Equals(definition.Name, expectedName, StringComparison.Ordinal))
        {
            throw new InvalidDataException(
                $"Agent definition '{path}' declares name '{definition.Name}', expected '{expectedName}'."
            );
        }

        if (string.IsNullOrWhiteSpace(definition.Description))
        {
            throw new InvalidDataException($"Agent definition '{path}' must include a description.");
        }

        var chatOptions = definition
            .GetChatOptionsAsync(new RecalcEngine(), functions: null)
            .GetAwaiter()
            .GetResult();

        if (chatOptions is null || string.IsNullOrWhiteSpace(chatOptions.Instructions))
        {
            throw new InvalidDataException(
                $"Agent definition '{path}' must include instructions."
            );
        }

        var toolNames =
            chatOptions.Tools?.Select(tool => tool.Name).ToHashSet(StringComparer.Ordinal)
            ?? new HashSet<string>(StringComparer.Ordinal);

        var options = new ChatClientAgentOptions
        {
            Name = definition.Name,
            Description = definition.Description,
            ChatOptions = chatOptions,
        };

        return (options, toolNames);
    }
}
