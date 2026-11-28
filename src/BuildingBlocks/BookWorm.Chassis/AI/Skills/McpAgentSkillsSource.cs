using System.Text.Json.Nodes;
using Microsoft.Agents.AI;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.Logging;
using ModelContextProtocol.Client;
using ModelContextProtocol.Extensions.Skills;

namespace BookWorm.Chassis.AI.Skills;

/// <summary>
///     An <see cref="AgentSkillsSource" /> that discovers and loads Agent Skills from an MCP server
///     implementing the MCP Skills extension (SEP-2640).
/// </summary>
public sealed class McpAgentSkillsSource : AgentSkillsSource
{
    private readonly ILogger<McpAgentSkillsSource>? _logger;
    private readonly McpClient _mcpClient;

    public McpAgentSkillsSource(McpClient mcpClient, ILoggerFactory? loggerFactory = null)
    {
        ArgumentNullException.ThrowIfNull(mcpClient);

        _mcpClient = mcpClient;
        _logger = loggerFactory?.CreateLogger<McpAgentSkillsSource>();
    }

    public override async Task<IList<AgentSkill>> GetSkillsAsync(
        AgentSkillsSourceContext context,
        CancellationToken cancellationToken = default
    )
    {
        if (!_mcpClient.ServerCapabilities.SupportsSkills())
        {
            _logger?.LogWarning(
                "MCP server '{ServerName}' does not advertise the Skills extension capability.",
                _mcpClient.ServerInfo?.Name ?? "unknown"
            );

            return [];
        }

        var remoteSkills = await _mcpClient.ListSkillsAsync(cancellationToken: cancellationToken);

        List<AgentSkill> skills = new(remoteSkills.Count);

        foreach (var remoteSkill in remoteSkills)
        {
            if (TryCreateFrontmatter(remoteSkill, out var frontmatter))
            {
                skills.Add(new McpAgentSkill(_mcpClient, remoteSkill, frontmatter));
            }
        }

        return skills;
    }

    private bool TryCreateFrontmatter(Skill skill, out AgentSkillFrontmatter frontmatter)
    {
        var name = skill.Name;
        var description = skill.Description;

        string? descriptionReason = null;

        if (
            !AgentSkillFrontmatter.ValidateName(name, out var nameReason)
            || !AgentSkillFrontmatter.ValidateDescription(description, out descriptionReason)
        )
        {
            _logger?.LogWarning(
                "Skipping MCP skill '{SkillUri}' due to invalid frontmatter: {Reason}",
                skill.Uri,
                nameReason ?? descriptionReason
            );
            frontmatter = null!;
            return false;
        }

        var compatibility = skill.Compatibility;
        if (
            compatibility is not null
            && !AgentSkillFrontmatter.ValidateCompatibility(compatibility, out var compatReason)
        )
        {
            _logger?.LogWarning(
                "Ignoring invalid compatibility field for MCP skill '{SkillUri}': {Reason}",
                skill.Uri,
                compatReason
            );
            compatibility = null;
        }

        // Security (SEP-2640): Never copy AllowedTools from MCP-origin skills without explicit approval.
        frontmatter = new(name!, description!, compatibility)
        {
            License = skill.License,
            Metadata = ExtractMetadata(skill.Frontmatter),
        };

        return true;
    }

    private static AdditionalPropertiesDictionary? ExtractMetadata(JsonObject frontmatter)
    {
        if (
            !frontmatter.TryGetPropertyValue("metadata", out var metadataNode)
            || metadataNode is not JsonObject metadataObject
        )
        {
            return null;
        }

        AdditionalPropertiesDictionary metadata = [];

        foreach (var (key, value) in metadataObject)
        {
            if (value is JsonValue jsonValue && jsonValue.TryGetValue(out string? text))
            {
                metadata[key] = text;
            }
            else if (value is not null)
            {
                metadata[key] = value.ToJsonString();
            }
        }

        return metadata.Count > 0 ? metadata : null;
    }
}
