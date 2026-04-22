namespace BookWorm.Chat.Agents.Basket;

internal static class BasketAgentDefinition
{
    public const string Name = Constants.Other.Agents.BasketAgent;

    /// <summary>
    /// Names of the client-side (AG-UI / CopilotKit) actions exposed by the storefront
    /// that this agent is allowed to invoke. The actual execution — including the
    /// confirmation dialog (Human-in-the-Loop) — happens on the client.
    /// </summary>
    public static readonly string[] ClientToolNames = ["addToBasket", "viewBasket"];

    public const string Description =
        "A Human-in-the-Loop (HITL) agent that detects customer purchase intent and proactively prompts the user to add a book to their basket. Adds the item via the storefront's AG-UI confirmation dialog only after explicit user approval.";

    public const string Instructions = """
        You assist BookWorm bookstore customers with shopping basket actions when they
        express interest in buying a book. You operate as a Human-in-the-Loop (HITL)
        agent: every basket modification requires explicit customer confirmation
        through the storefront's confirmation dialog before it is applied.

        Capabilities:
        - Detect purchase intent (e.g. "I want to buy this", "add it to my cart",
          "I'll take two copies of …", "let me get this one").
        - Use the addToBasket client tool to propose adding a book to the basket.
          The storefront shows a confirmation dialog and only adds the item if the
          user accepts. Always pass the bookId and, when known, bookTitle, price,
          and quantity (default 1) so the dialog is informative.
        - Use the viewBasket client tool to show the customer the current contents
          of their basket on request.

        Behavior:
        - Be concise, friendly, and helpful.
        - Confirm the book and quantity in your reply before invoking addToBasket so
          the customer knows what is about to be proposed.
        - Never assume consent. If the customer's intent is ambiguous, ask a short
          clarifying question first (e.g. "Would you like me to add 1 copy of
          '<title>' to your basket?").
        - If the customer cancels in the confirmation dialog, acknowledge politely
          and offer alternatives (continue browsing, view basket, get more details).
        - Do not modify the basket more than once per turn unless the user explicitly
          asked for multiple items.

        Handoff:
        - Hand off to BookAgent if the customer wants more book details, search, or
          recommendations before deciding what to buy.
        - Hand off to QAAgent for shipping, returns, payment, or account questions.
        - Hand off back to RouterAgent for any topic outside basket management.

        Goal: Make the path from "I like this book" to "it's in my basket" smooth and
        explicit, while always respecting the customer's final confirmation.
        """;

    public static AgentCard AgentCard { get; } =
        new()
        {
            Name = Name,
            Description = Description,
            Version = "1.0",
            Provider = new() { Organization = nameof(OpenAI), Url = "https://openai.com/" },
            DefaultInputModes = ["text"],
            DefaultOutputModes = ["text"],
            Capabilities = new() { Streaming = false, PushNotifications = false },
            Skills =
            [
                new()
                {
                    Id = "basket_agent_purchase_intent",
                    Tags = ["basket", "purchase", "intent"],
                    Name = "Purchase Intent Detection",
                    Description =
                        "Detect when a customer expresses interest in buying a book and propose adding it to their basket",
                    Examples =
                    [
                        "I want to buy this book",
                        "Add it to my cart",
                        "I'll take two copies of The Pragmatic Programmer",
                    ],
                },
                new()
                {
                    Id = "basket_agent_add_to_basket",
                    Tags = ["basket", "add", "checkout"],
                    Name = "Add to Basket (HITL)",
                    Description =
                        "Propose adding a book to the customer's basket via the storefront's confirmation dialog; the item is only added after the customer explicitly confirms",
                    Examples =
                    [
                        "Please add 'Clean Code' to my basket",
                        "Add 3 of these to the cart",
                        "Buy this one for me",
                    ],
                },
                new()
                {
                    Id = "basket_agent_view_basket",
                    Tags = ["basket", "view", "summary"],
                    Name = "View Basket Contents",
                    Description =
                        "Show the current contents of the customer's basket including items, quantities and total price",
                    Examples =
                    [
                        "What's in my basket?",
                        "Show me my cart",
                        "How much is in my basket right now?",
                    ],
                },
            ],
            PreferredTransport = AgentTransport.JsonRpc,
        };
}
