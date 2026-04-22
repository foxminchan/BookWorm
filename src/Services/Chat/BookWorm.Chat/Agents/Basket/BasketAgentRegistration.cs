using BookWorm.Chassis.AI.Middlewares;
using Microsoft.Agents.AI;
using Microsoft.Agents.AI.Hosting;

namespace BookWorm.Chat.Agents.Basket;

/// <summary>
///     Registers the Basket agent with the application DI container.
/// </summary>
internal static class BasketAgentRegistration
{
    /// <summary>
    ///     Adds the Basket agent to the application builder with HITL-gated AddToBasket tool.
    /// </summary>
    /// <param name="builder">The application host builder.</param>
    public static void AddBasketAgent(this IHostApplicationBuilder builder)
    {
        builder.AddAIAgent(
            BasketAgentDefinition.Name,
            (sp, key) =>
            {
                var chatClient = sp.GetRequiredService<IChatClient>()
                    .AsBuilder()
                    .UsePIIMiddleware(sp)
                    .UseGuardrailMiddleware()
                    .UseGovernanceToolCall(sp, BasketAgentDefinition.Name)
                    .Build(sp);

                // Wrap AddToBasket with HITL approval gate — requires explicit customer confirmation
                AITool addToBasketTool = new ApprovalRequiredAIFunction(
                    AIFunctionFactory.Create(
                        (
                            string bookId,
                            int quantity,
                            IHttpClientFactory httpClientFactory,
                            System.Security.Claims.ClaimsPrincipal claimsPrincipal,
                            CancellationToken ct
                        ) =>
                            BasketAgentDefinition.AddToBasketAsync(
                                bookId,
                                quantity,
                                httpClientFactory,
                                claimsPrincipal,
                                ct
                            ),
                        "AddToBasket",
                        "Add a book to the customer's basket. REQUIRES explicit customer approval."
                    )
                );

                var agent = new ChatClientAgent(
                    chatClient,
                    options: new()
                    {
                        Name = key,
                        Description = BasketAgentDefinition.Description,
                        ChatOptions = new()
                        {
                            Instructions = BasketAgentDefinition.Instructions,
                            Temperature = 0.3f,
                            MaxOutputTokens = 500,
                            AllowMultipleToolCalls = false,
                            Tools = [addToBasketTool],
                        },
                    }
                );

                return agent;
            }
        );
    }
}
