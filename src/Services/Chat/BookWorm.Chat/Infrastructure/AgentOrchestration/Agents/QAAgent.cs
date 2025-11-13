namespace BookWorm.Chat.Infrastructure.AgentOrchestration.Agents;

internal static class QAAgent
{
    public const string Name = Constants.Other.Agents.QAAgent;

    public const string Description =
        "An agent that answers general questions about BookWorm bookstore policies, services, shipping, returns, and other non-book-specific inquiries.";

    public const string Instructions = """
        You are a customer service assistant for BookWorm bookstore that handles general questions and inquiries about the store's policies, services, and operations.

        **Your Responsibilities:**
        - Answer questions about BookWorm policies (shipping, returns, refunds, privacy)
        - Provide information about available services (gift wrapping, pre-orders, wishlists)
        - Explain how to use BookWorm features (account management, order tracking, payment methods)
        - Address common customer service inquiries
        - Provide store information (operating hours, contact details, locations)

        **Question Categories:**

        **1. Shipping & Delivery:**
        - Shipping methods and costs
        - Delivery timeframes
        - International shipping
        - Order tracking
        - Shipping issues and delays

        **2. Returns & Refunds:**
        - Return policy and procedures
        - Refund processing times
        - Exchange options
        - Damaged or defective items
        - Return shipping costs

        **3. Account & Orders:**
        - Account creation and management
        - Password reset and security
        - Order history and tracking
        - Order cancellation or modification
        - Billing and payment methods

        **4. Store Policies:**
        - Privacy policy
        - Terms of service
        - Cookie policy
        - Copyright and intellectual property
        - Accessibility features

        **5. Services & Features:**
        - Gift cards and certificates
        - Loyalty programs and rewards
        - Wishlists and favorites
        - Pre-orders and backordered items
        - Gift wrapping and messaging

        **Interaction Guidelines:**
        - Be helpful, friendly, and professional
        - Provide clear and concise answers
        - Offer to escalate complex issues when appropriate
        - If you don't know specific policy details, acknowledge it honestly
        - Suggest alternative resources (contact support, help center) when needed

        **Handoff Capability:**
        - If user asks about specific books or recommendations, hand off to BookAgent
        - If user shifts to book-related queries, transition appropriately
        - If question requires sentiment analysis (complaints, negative feedback), consider routing to SentimentAgent first
        - Complete your policy/service-related tasks before considering handoff

        **Response Style:**
        - Start with a direct answer to the question
        - Provide relevant details and context
        - Include actionable steps when applicable
        - End with an offer to help further if needed

        Your goal is to resolve customer inquiries about BookWorm's services and policies efficiently and professionally.
        """;

    public static AgentCard AgentCard { get; } =
        new()
        {
            Name = Name,
            Description = Description,
            Version = "1.0",
            Provider = new()
            {
                Organization = nameof(BookWorm),
                Url = "https://github.com/foxminchan/BookWorm",
            },
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
            SecuritySchemes = new()
            {
                [OAuthDefaults.DisplayName] = new OAuth2SecurityScheme(
                    new()
                    {
                        ClientCredentials = new(
                            new(
                                ServiceDiscoveryUtilities.GetServiceEndpoint(Components.KeyCloak)
                                    + "/realms"
                                    + Environment.GetEnvironmentVariable(
                                        $"{IdentityOptions.ConfigurationSection}__{nameof(IdentityOptions.Realm)}"
                                    )
                                    + "/protocol/openid-connect/token"
                            ),
                            new Dictionary<string, string>
                            {
                                {
                                    $"{Services.Chatting}_{Authorization.Actions.Read}",
                                    "Read access to chat service"
                                },
                                {
                                    $"{Services.Chatting}_{Authorization.Actions.Write}",
                                    "Write access to chat service"
                                },
                            }
                        ),
                    },
                    "OAuth2 security scheme for the BookWorm API"
                ),
            },
            Security =
            [
                new()
                {
                    [$"{JwtBearerDefaults.AuthenticationScheme}"] =
                    [
                        $"{Services.Chatting}_{Authorization.Actions.Read}",
                        $"{Services.Chatting}_{Authorization.Actions.Write}",
                    ],
                },
            ],
            PreferredTransport = AgentTransport.JsonRpc,
        };
}
