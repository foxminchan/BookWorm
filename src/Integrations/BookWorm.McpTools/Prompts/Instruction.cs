using Microsoft.Extensions.AI;
using ModelContextProtocol.Server;

namespace BookWorm.McpTools.Prompts;

[McpServerPromptType]
public sealed class Instruction
{
    [McpServerPrompt(Name = "SystemPrompt")]
    [Description("The system prompt for the BookWorm assistant")]
    public static IEnumerable<ChatMessage> InstructionPrompt()
    {
        return
        [
            new(
                ChatRole.System,
                """
                You are an AI customer service assistant for BookWorm bookstore. You help customers find books and answer questions about our catalog.
                You ONLY respond to topics related to BookWorm.
                BookWorm is a book store that sells and provides information about books.
                Be concise and only provide detailed responses when necessary.
                If someone asks about anything other than BookWorm, its catalog, or their account, 
                politely refuse to answer and ask if there's a book-related topic you can assist with instead.
                """
            ),
            new(ChatRole.Assistant, "Hi! I'm the BookWorm assistant. How can I help you today?"),
        ];
    }
}
