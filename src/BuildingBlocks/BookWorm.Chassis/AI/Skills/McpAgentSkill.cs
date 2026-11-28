using Microsoft.Agents.AI;
using ModelContextProtocol.Client;
using ModelContextProtocol.Extensions.Skills;
using ModelContextProtocol.Protocol;

namespace BookWorm.Chassis.AI.Skills;

internal sealed class McpAgentSkill : AgentSkill
{
    private readonly McpClient _mcpClient;
    private readonly Dictionary<string, SkillResource> _resourcesByPath;
    private readonly Skill _skill;

    public McpAgentSkill(McpClient mcpClient, Skill skill, AgentSkillFrontmatter frontmatter)
    {
        ArgumentNullException.ThrowIfNull(mcpClient);
        ArgumentNullException.ThrowIfNull(skill);
        ArgumentNullException.ThrowIfNull(frontmatter);

        _mcpClient = mcpClient;
        _skill = skill;
        Frontmatter = frontmatter;
        _resourcesByPath = BuildResourceMap(skill);
    }

    public override AgentSkillFrontmatter Frontmatter { get; }

    public override async ValueTask<string> GetContentAsync(
        CancellationToken cancellationToken = default
    )
    {
        var result = await _mcpClient.ReadSkillResourceAsync(
            _skill,
            _skill.Uri,
            cancellationToken: cancellationToken
        );

        var expectedUri = new Uri(_skill.Uri);
        var content = result.Contents.Single(c => new Uri(c.Uri) == expectedUri);

        return content is TextResourceContents textContent
            ? textContent.Text
            : throw new InvalidDataException(
                $"Skill '{_skill.Uri}' must return text content for SKILL.md."
            );
    }

    public override ValueTask<AgentSkillResource?> GetResourceAsync(
        string name,
        CancellationToken cancellationToken = default
    )
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            return ValueTask.FromResult<AgentSkillResource?>(null);
        }

        var normalized = NormalizeResourcePath(name);

        if (!_resourcesByPath.TryGetValue(normalized, out var entry))
        {
            return ValueTask.FromResult<AgentSkillResource?>(null);
        }

        AgentSkillResource resource = new McpAgentSkillResource(
            _mcpClient,
            _skill,
            entry.Uri,
            normalized
        );

        return ValueTask.FromResult<AgentSkillResource?>(resource);
    }

    private static Dictionary<string, SkillResource> BuildResourceMap(Skill skill)
    {
        Dictionary<string, SkillResource> map = new(StringComparer.OrdinalIgnoreCase);

        if (skill.Resources.IsDynamic || skill.Resources.Entries is not { Count: > 0 } entries)
        {
            return map;
        }

        var skillUri = new Uri(skill.Uri);
        var baseUri = GetSkillBaseUri(skill.Uri);

        foreach (var entry in entries)
        {
            if (new Uri(entry.Uri) == skillUri)
            {
                continue;
            }

            if (entry.Uri.StartsWith(baseUri, StringComparison.OrdinalIgnoreCase))
            {
                var relativePath = Uri.UnescapeDataString(entry.Uri[baseUri.Length..]);
                map[NormalizeResourcePath(relativePath)] = entry;
            }
        }

        return map;
    }

    private static string GetSkillBaseUri(string skillUri)
    {
        var lastSlash = skillUri.LastIndexOf('/');
        return lastSlash >= 0 ? skillUri[..(lastSlash + 1)] : skillUri;
    }

    private static string NormalizeResourcePath(string path)
    {
        var normalized = path.Replace('\\', '/').Trim();
        while (normalized.StartsWith("./", StringComparison.Ordinal))
        {
            normalized = normalized[2..];
        }

        return normalized.TrimStart('/');
    }
}
