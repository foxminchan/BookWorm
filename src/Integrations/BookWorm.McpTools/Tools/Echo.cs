using ModelContextProtocol.Server;

namespace BookWorm.McpTools.Tools;

[McpServerToolType]
public sealed class Echo
{
    [McpMeta("category", "ping")]
    [McpServerTool(Name = "echo", Title = "Echo Tool")]
    [Description("Echoes back the provided input string.")]
    [return: Description("The echoed string with a greeting.")]
    public string EchoInput([Description("The string to be echoed back.")] string input)
    {
        return string.Concat("Hello from BookWorm! You said: ", input);
    }
}
