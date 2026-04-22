using A2A;

namespace BookWorm.Rating.Infrastructure.Agents;

internal static class RatingAgent
{
    public const string Name = Constants.Other.Agents.RatingAgent;

    public const string Description =
        "Analyzes book ratings, classifies product quality, and enables customers to submit or update their reviews.";

    public const string Instructions = """
        You analyze book ratings data, classify product quality, and help customers submit reviews.

        Tasks:
        1. Analyze rating data (average, total reviews, distribution)
        2. Calculate summary statistics
        3. Classify quality: Best Seller/Good/Bad/No Data
        4. Provide sentiment analysis and summarization
        5. Accept and submit customer reviews (requires explicit customer approval via HITL)

        Available Functions:
        - GetCustomerReviews: Retrieve customer feedback data (all raw reviews for a book)
        - submitReview: Submit or replace a review for a book (HITL-gated — requires customer confirmation)
        - get_book: Retrieve book metadata from the catalog (title, authors, category, price, catalog-computed AverageRating and TotalReviews)
        - SummarizeAgent: Text processing, sentiment analysis, summarization
        Select and combine functions based on analysis needs.
        Always call get_book first to obtain book metadata, then call GetCustomerReviews for the raw review data.

        Review Submission:
        - When a customer wants to submit a review, collect: bookId, rating (1-5), optional comment
        - Summarise the review back to the customer for confirmation before submitting
        - Use submitReview — customer must explicitly confirm before execution
        - On success, congratulate and offer to help further
        - On cancellation, acknowledge politely

        Classification Rules:
        - Best Seller: Avg ≥4.5 + 50+ reviews, OR ≥4.0 + 200+ reviews
        - Good: Avg ≥3.5 + 10+ reviews
        - Bad: Avg <3.5 or negative feedback patterns
        - No Data: No ratings available

        Output:
        - Classification (Best Seller/Good/Bad/No Data)
        - Metrics (avg rating, review count, distribution)
        - Brief justification
        - Confidence level

        Considerations:
        - Weight recent ratings higher
        - Check review quality/authenticity
        - Account for seasonal trends
        - Handle edge cases (few reviews, high ratings)
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
                    Id = "rating_agent_analyze",
                    Tags = ["ratings", "analysis", "classification", "quality"],
                    Name = "Rating Analysis",
                    Description =
                        "Analyzes book ratings data and classifies product quality as Best Seller, Good, Bad, or No Data.",
                    Examples =
                    [
                        "How is this book rated?",
                        "Is this book popular?",
                        "What do customers think of this book?",
                    ],
                    InputModes = ["text"],
                    OutputModes = ["text"],
                },
                new()
                {
                    Id = "rating_agent_submit_review",
                    Tags = ["review", "rating", "submit", "feedback"],
                    Name = "Submit Review",
                    Description =
                        "Helps customers submit or update a star rating and optional comment for a book. Requires explicit customer approval before submission.",
                    Examples =
                    [
                        "I want to leave a review for this book",
                        "Submit a 4-star review",
                        "Update my review for this book",
                    ],
                    InputModes = ["text"],
                    OutputModes = ["text"],
                },
            ],
        };
}
