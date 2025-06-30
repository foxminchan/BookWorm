using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.Agents;

namespace BookWorm.Chat.Agents;

public class BookAgent
{
    private const string Name = nameof(BookAgent);
    private const string Description =
        "An agent that searches for books based on user queries and provides relevant information.";
    private const string Instructions = """
        You are an AI assistant for BookWorm bookstore. Given a user query, search the BookWorm catalog and provide relevant book information. 
        Do not include details about books not present in the catalog.
        """;

    public Agent CreateAgent(Kernel kernel)
    {
        return new ChatCompletionAgent
        {
            Instructions = Instructions,
            Name = Name,
            Description = Description,
            Kernel = kernel,
        };
    }
}
