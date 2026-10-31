using Microsoft.Agents.AI;
using Microsoft.Agents.ObjectModel;
using Microsoft.Agents.ObjectModel.Yaml;
using Microsoft.Extensions.AI;

namespace BookWorm.Chassis.AI.Agents;

public static class AgentDefinitionLoader
{
    private const string ReasoningEffortOption = "reasoningEffort";

    /// <summary>
    ///     Loads a prompt agent definition from the application output directory.
    /// </summary>
    /// <param name="definitionFile">The path to the definition, relative to the application output directory.</param>
    /// <param name="expectedName">The agent name expected by the registering service.</param>
    /// <returns>The configured options and declared tool names.</returns>
    /// <remarks>
    ///     A declared <c>reasoningEffort</c> model option is applied to the typed chat reasoning options.
    /// </remarks>
    /// <exception cref="InvalidDataException">
    ///     The definition is not a prompt agent, its name does not match, required metadata is missing,
    ///     or its reasoning effort option is invalid.
    /// </exception>
    public static (ChatClientAgentOptions Options, IReadOnlySet<string> ToolNames) Load(
        string definitionFile,
        string expectedName
    )
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
            throw new InvalidDataException(
                $"Agent definition '{path}' must include a description."
            );
        }

        var declaredReasoningEffort =
            definition.Model.Options?.ExtensionData?.Properties.GetValueOrDefault(
                ReasoningEffortOption
            );

        if (declaredReasoningEffort is not null and not StringDataValue)
        {
            throw new InvalidDataException(
                $"Agent definition '{path}' has an invalid model option '{ReasoningEffortOption}'."
            );
        }

        var chatOptions = definition.GetChatOptionsAsync(new(), null).GetAwaiter().GetResult();

        if (chatOptions is null || string.IsNullOrWhiteSpace(chatOptions.Instructions))
        {
            throw new InvalidDataException($"Agent definition '{path}' must include instructions.");
        }

        if (
            chatOptions.AdditionalProperties?.TryGetValue(
                ReasoningEffortOption,
                out var reasoningEffortValue
            )
                is true
            && reasoningEffortValue is not null
        )
        {
            if (
                reasoningEffortValue is not string reasoningEffortName
                || !Enum.TryParse<ReasoningEffort>(
                    reasoningEffortName,
                    true,
                    out var reasoningEffort
                )
                || !Enum.IsDefined(reasoningEffort)
            )
            {
                throw new InvalidDataException(
                    $"Agent definition '{path}' has an invalid model option '{ReasoningEffortOption}'."
                );
            }

            (chatOptions.Reasoning ??= new()).Effort = reasoningEffort;
        }

        var toolNames =
            chatOptions.Tools?.Select(tool => tool.Name).ToHashSet(StringComparer.Ordinal)
            ?? [with(StringComparer.Ordinal)];

        var options = new ChatClientAgentOptions
        {
            Name = definition.Name,
            Description = definition.Description,
            ChatOptions = chatOptions,
        };

        return (options, toolNames);
    }
}
