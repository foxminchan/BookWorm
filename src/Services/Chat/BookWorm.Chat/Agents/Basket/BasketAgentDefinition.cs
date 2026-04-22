using System.ComponentModel;
using System.Net.Http.Json;
using System.Security.Claims;

namespace BookWorm.Chat.Agents.Basket;

/// <summary>
///     Definition and tool implementation for the Basket agent.
/// </summary>
internal static class BasketAgentDefinition
{
    /// <summary>The registered agent name used for keyed DI and workflow routing.</summary>
    public const string Name = Constants.Other.Agents.BasketAgent;

    /// <summary>Short agent description surfaced in agent discovery.</summary>
    public const string Description =
        "An agent that helps customers add books to their shopping basket. Requires explicit customer approval before performing any basket writes.";

    /// <summary>System-level instruction prompt for the basket agent.</summary>
    public const string Instructions = """
        You help BookWorm customers add books to their shopping basket.

        Capabilities:
        - Add a specific book (by ID) to the customer's basket, with a chosen quantity
        - Always present the book title and price to the customer before adding

        Behavior:
        - Never add a book to the basket without explicit customer approval (HITL gate)
        - Always use the AddToBasket tool; do not invent basket mutations
        - Confirm success or report errors clearly
        - Hand off back to RouterAgent after basket operation completes or is cancelled

        Safety:
        - A human approval step is required before the basket is modified
        - If the customer declines, acknowledge and offer alternatives
        """;

    /// <summary>
    ///     Adds a book to the customer's shopping basket via PUT /api/v1/baskets.
    ///     Requires HITL approval before this function executes.
    /// </summary>
    /// <param name="bookId">The unique identifier (UUID) of the book.</param>
    /// <param name="quantity">Number of copies to add (minimum 1).</param>
    /// <param name="httpClientFactory">Injected HTTP client factory for calling the Basket service.</param>
    /// <param name="claimsPrincipal">Injected authenticated user claims.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>A human-readable result message.</returns>
    [Description(
        "Add a book to the customer's shopping basket. This action REQUIRES explicit customer approval and will not execute until the customer confirms."
    )]
    internal static async Task<string> AddToBasketAsync(
        [Description("The UUID of the book to add")] string bookId,
        [Description("Number of copies to add (minimum 1, maximum 99)")] int quantity,
        IHttpClientFactory httpClientFactory,
        ClaimsPrincipal claimsPrincipal,
        CancellationToken cancellationToken = default
    )
    {
        if (quantity < 1)
        {
            return "Quantity must be at least 1.";
        }

        if (quantity > 99)
        {
            return "Quantity cannot exceed 99.";
        }

        if (!Guid.TryParse(bookId, out _))
        {
            return $"Invalid book ID format: {bookId}.";
        }

        var httpClient = httpClientFactory.CreateClient(Services.Basket);

        var payload = new { items = new[] { new { id = bookId, quantity } } };

        using var response = await httpClient.PutAsJsonAsync(
            "api/v1/baskets",
            payload,
            cancellationToken
        );

        if (response.IsSuccessStatusCode)
        {
            return $"Successfully added {quantity} {(quantity == 1 ? "copy" : "copies")} (book {bookId}) to your basket.";
        }

        var errorBody = await response.Content.ReadAsStringAsync(cancellationToken);
        return $"Failed to update basket (HTTP {(int)response.StatusCode}): {errorBody}";
    }
}
