using A2A;
using BookWorm.Chassis.Utilities;
using BookWorm.Constants.Core;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authentication.OAuth;

namespace BookWorm.Rating.Infrastructure.Agents;

internal static class RatingAgent
{
    public const string Name = Constants.Other.Agents.RatingAgent;

    public const string Description =
        "Summarizes book ratings and evaluates product quality as bad, good, or best seller.";

    public const string Instructions = """
        You are a Rating Agent responsible for processing and evaluating book ratings data with contextual intelligence.

        **Primary Responsibilities**:
        1. Analyze aggregated rating data for books/products using contextually selected functions
        2. Calculate summary statistics (average rating, total reviews, rating distribution)
        3. Evaluate and classify product quality based on rating metrics
        4. Provide sentiment analysis and content summarization

        **Available Functions for Contextual Selection**:
        - **GetCustomerReviews**: Retrieves raw customer review data for analysis
        - **SummarizeAgent functions**: Provides advanced text processing, summarization, and sentiment analysis

        **Contextual Function Usage Guidelines**:
        - Use GetCustomerReviews when you need actual customer feedback data
        - Use SummarizeAgent functions for content analysis, sentiment evaluation, and text processing
        - Select functions based on the specific analysis requirements and context
        - Combine multiple functions for comprehensive rating assessment

        **Classification Rules**:
        - **Best Seller**: Average rating ≥ 4.5 with at least 50 reviews, or ≥ 4.0 with 200+ reviews
        - **Good**: Average rating ≥ 3.5 with at least 10 reviews
        - **Bad**: Average rating < 3.5 or predominantly negative feedback patterns
        - **No Data**: No ratings available for the product

        **Output Format**:
        Always provide:
        - Classification result (Best Seller/Good/Bad/No Data)
        - Supporting metrics (average rating, review count, rating distribution)
        - Brief justification for the classification
        - Confidence level in the assessment

        **Special Considerations**:
        - Weight recent ratings more heavily than older ones
        - Consider review quality and authenticity indicators
        - Account for seasonal trends and promotional periods
        - Handle edge cases like products with very few but high ratings
        - Use contextual intelligence to select the most relevant functions for each analysis task
        """;

    public static AgentCard AgentCard { get; } =
        new()
        {
            Name = Name,
            Description =
                "Rating analysis agent with contextual function selection using existing plugins and A2A integration",
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
                    Id = "rating_agent_rating_analysis",
                    Tags = ["analysis", "rating", "statistics"],
                    Name = "Rating Analysis",
                    Description =
                        "Analyze aggregated rating data for books including average rating, total reviews, and rating distribution",
                    Examples =
                    [
                        "What is the average rating for the book 'The Great Gatsby'?",
                        "How many reviews does 'To Kill a Mockingbird' have?",
                        "Show me the rating distribution for '1984'",
                    ],
                },
                new()
                {
                    Id = "rating_agent_quality_classification",
                    Tags = ["classification", "quality", "rating"],
                    Name = "Quality Classification",
                    Description =
                        "Evaluate and classify product quality as Best Seller, Good, Bad, or No Data based on rating metrics",
                    Examples =
                    [
                        "Classify the product quality of '1984' by George Orwell.",
                        "Is 'Pride and Prejudice' a best seller based on ratings?",
                        "What quality category does this book fall into?",
                    ],
                },
                new()
                {
                    Id = "rating_agent_sentiment_analysis",
                    Tags = ["sentiment", "analysis", "reviews"],
                    Name = "Sentiment Analysis",
                    Description =
                        "Analyze customer sentiment and provide content summarization from book reviews",
                    Examples =
                    [
                        "Analyze customer sentiment for reviews of a specific book.",
                        "What are customers saying about 'The Catcher in the Rye'?",
                        "Summarize the main themes in reviews for this book",
                    ],
                },
                new()
                {
                    Id = "rating_agent_comprehensive_report",
                    Tags = ["report", "comprehensive", "analysis"],
                    Name = "Comprehensive Rating Report",
                    Description =
                        "Provide comprehensive rating analysis with quality classification, metrics, and justification",
                    Examples =
                    [
                        "Provide comprehensive rating analysis with quality classification.",
                        "Give me a full rating report for 'Harry Potter and the Sorcerer's Stone'",
                        "Analyze all rating aspects for this book including classification and sentiment",
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
                                    $"{Constants.Aspire.Services.Rating}_{Authorization.Actions.Read}",
                                    "Read access to rating service"
                                },
                                {
                                    $"{Constants.Aspire.Services.Rating}_{Authorization.Actions.Write}",
                                    "Write access to rating service"
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
                        $"{Constants.Aspire.Services.Rating}_{Authorization.Actions.Read}",
                        $"{Constants.Aspire.Services.Rating}_{Authorization.Actions.Write}",
                    ],
                },
            ],
            PreferredTransport = AgentTransport.JsonRpc,
        };
}
