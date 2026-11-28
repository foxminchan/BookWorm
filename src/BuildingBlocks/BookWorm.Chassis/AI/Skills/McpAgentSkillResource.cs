using System.Text;
using Microsoft.Agents.AI;
using ModelContextProtocol.Client;
using ModelContextProtocol.Extensions.Skills;
using ModelContextProtocol.Protocol;

namespace BookWorm.Chassis.AI.Skills;

internal sealed class McpAgentSkillResource(
    McpClient mcpClient,
    Skill skill,
    string resourceUri,
    string name
) : AgentSkillResource(name)
{
    public override async Task<object?> ReadAsync(
        IServiceProvider? serviceProvider = null,
        CancellationToken cancellationToken = default
    )
    {
        var result = await mcpClient.ReadSkillResourceAsync(
            skill,
            resourceUri,
            cancellationToken: cancellationToken
        );

        var expectedUri = new Uri(resourceUri);
        var content = result.Contents.Single(c => new Uri(c.Uri) == expectedUri);

        return content switch
        {
            TextResourceContents text => text.Text,
            BlobResourceContents blob => Encoding.UTF8.GetString(blob.DecodedData.Span),
            _ => throw new NotSupportedException(
                $"Unsupported resource content type '{content.GetType().Name}' for '{resourceUri}'."
            ),
        };
    }
}
