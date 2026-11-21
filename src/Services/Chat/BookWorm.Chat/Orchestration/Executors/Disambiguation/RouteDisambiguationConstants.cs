using BookWorm.Chat.Infrastructure.Decisions;

namespace BookWorm.Chat.Orchestration.Executors.Disambiguation;

internal static class RouteDisambiguationConstants
{
    public const string ToolName = "disambiguateRoute";
    public const string PortName = "RouteDisambiguationPort";

    public static IReadOnlyList<RouteDisambiguationOption> DefaultOptions { get; } =
    [
        new(
            Constants.Other.Agents.BookAgent,
            "Book Search & Recommendations",
            "Find books, get recommendations, or add books to your basket."
        ),
        new(
            Constants.Other.Agents.QAAgent,
            "Store Policies & Support",
            "Shipping, returns, orders, accounts, or general bookstore policies."
        ),
        new(
            Constants.Other.Agents.RatingAgent,
            "Reviews & Ratings",
            "Summarize customer reviews or analyze book ratings."
        ),
    ];

    public static string ToAgentId(RouteTarget target)
    {
        return target switch
        {
            RouteTarget.QA => Constants.Other.Agents.QAAgent,
            RouteTarget.Rating => Constants.Other.Agents.RatingAgent,
            _ => Constants.Other.Agents.BookAgent,
        };
    }

    public static bool TryParseTarget(string? value, out RouteTarget target)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            target = default;
            return false;
        }

        var normalized = value.Trim();

        if (
            string.Equals(
                normalized,
                Constants.Other.Agents.BookAgent,
                StringComparison.OrdinalIgnoreCase
            )
            || string.Equals(
                normalized,
                nameof(RouteTarget.Book),
                StringComparison.OrdinalIgnoreCase
            )
            || normalized.Contains("book", StringComparison.OrdinalIgnoreCase)
            || normalized == "1"
        )
        {
            target = RouteTarget.Book;
            return true;
        }

        if (
            string.Equals(
                normalized,
                Constants.Other.Agents.QAAgent,
                StringComparison.OrdinalIgnoreCase
            )
            || string.Equals(normalized, nameof(RouteTarget.QA), StringComparison.OrdinalIgnoreCase)
            || normalized.Contains("qa", StringComparison.OrdinalIgnoreCase)
            || normalized.Contains("support", StringComparison.OrdinalIgnoreCase)
            || normalized.Contains("policy", StringComparison.OrdinalIgnoreCase)
            || normalized.Contains("policies", StringComparison.OrdinalIgnoreCase)
            || normalized == "2"
        )
        {
            target = RouteTarget.QA;
            return true;
        }

        if (
            string.Equals(
                normalized,
                Constants.Other.Agents.RatingAgent,
                StringComparison.OrdinalIgnoreCase
            )
            || string.Equals(
                normalized,
                nameof(RouteTarget.Rating),
                StringComparison.OrdinalIgnoreCase
            )
            || normalized.Contains("rating", StringComparison.OrdinalIgnoreCase)
            || normalized.Contains("review", StringComparison.OrdinalIgnoreCase)
            || normalized == "3"
        )
        {
            target = RouteTarget.Rating;
            return true;
        }

        target = default;
        return false;
    }
}
