namespace BookWorm.Chat.Infrastructure.AgentOrchestration.Agents;

internal static class QAAgent
{
    public const string Name = Constants.Other.Agents.QAAgent;

    public const string Description =
        "An agent that answers general questions about BookWorm bookstore policies, services, shipping, returns, and other non-book-specific inquiries.";

    public const string Instructions = """
        You handle customer service inquiries for BookWorm bookstore about policies, services, and operations.

        Topics Covered:
        - Shipping & Delivery: methods, costs, timeframes, tracking, international, delays
        - Returns & Refunds: policy, procedures, processing times, exchanges, damaged items
        - Account & Orders: management, password reset, order tracking/cancellation, billing, payment
        - Store Policies: privacy, terms, cookies, copyright, accessibility
        - Services & Features: gift cards, loyalty programs, wishlists, pre-orders, gift wrapping

        Response Guidelines:
        - Be helpful, friendly, and professional
        - Give direct answers with relevant details and actionable steps
        - Acknowledge when you don't know specific details
        - Suggest escalation or alternative resources (support, help center) when needed
        - Offer further assistance at end

        Handoff Rules:
        - Hand off to BookAgent for book-specific queries or recommendations
        - Route to SentimentAgent for complaints/negative feedback
        - Complete current tasks before handoff

        Goal: Resolve inquiries efficiently and professionally.
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
                    Id = "qa_agent_shipping_delivery",
                    Tags = ["shipping", "delivery", "tracking"],
                    Name = "Shipping & Delivery Information",
                    Description =
                        "Provide information about shipping methods, delivery times, and order tracking",
                    Examples =
                    [
                        "What are your shipping options?",
                        "How long does delivery take?",
                        "Can I track my order?",
                    ],
                },
                new()
                {
                    Id = "qa_agent_returns_refunds",
                    Tags = ["returns", "refunds", "exchanges"],
                    Name = "Returns & Refunds Assistance",
                    Description =
                        "Handle questions about return policies, refund procedures, and exchanges",
                    Examples =
                    [
                        "What is your return policy?",
                        "How do I return a book?",
                        "When will I get my refund?",
                    ],
                },
                new()
                {
                    Id = "qa_agent_account_management",
                    Tags = ["account", "orders", "billing"],
                    Name = "Account & Order Management",
                    Description =
                        "Assist with account management, order tracking, and billing inquiries",
                    Examples =
                    [
                        "How do I reset my password?",
                        "Where is my order?",
                        "How can I update my billing information?",
                    ],
                },
                new()
                {
                    Id = "qa_agent_policies_services",
                    Tags = ["policies", "services", "features"],
                    Name = "Policies & Services Information",
                    Description =
                        "Explain store policies, available services, and special features",
                    Examples =
                    [
                        "What is your privacy policy?",
                        "Do you offer gift wrapping?",
                        "How does the loyalty program work?",
                    ],
                },
            ],
            PreferredTransport = AgentTransport.JsonRpc,
        };
}
