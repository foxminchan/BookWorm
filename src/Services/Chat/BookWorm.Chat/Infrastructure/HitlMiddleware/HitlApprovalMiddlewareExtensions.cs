using System.Text.Json;

namespace BookWorm.Chat.Infrastructure.HitlMiddleware;

/// <summary>
///     Extension utilities supporting the HITL approval protocol between the Chat service and
///     the storefront frontend via the AG-UI / CopilotKit channel.
/// </summary>
/// <remarks>
///     The AG-UI layer (MapAGUI) automatically surfaces <see cref="FunctionApprovalRequestContent" />
///     as tool-call events to CopilotKit. These helpers provide serialization utilities for building
///     and parsing approval payloads carried inside those events.
/// </remarks>
internal static class HitlApprovalMiddlewareExtensions
{
    private static readonly JsonSerializerOptions _serializerOptions = new(
        ApprovalJsonContext.Default.Options
    );

    /// <summary>
    ///     Serializes an <see cref="ApprovalRequest" /> to a JSON string for embedding in an
    ///     AG-UI tool-call argument payload.
    /// </summary>
    /// <param name="request">The approval request to serialize.</param>
    /// <returns>JSON string representation.</returns>
    public static string ToJson(this ApprovalRequest request) =>
        JsonSerializer.Serialize(request, ApprovalJsonContext.Default.ApprovalRequest);

    /// <summary>
    ///     Attempts to deserialize an <see cref="ApprovalResponse" /> from the raw JSON returned
    ///     by the frontend confirmation dialog.
    /// </summary>
    /// <param name="json">Raw JSON from the frontend.</param>
    /// <param name="response">Parsed response, or <see langword="null" /> if parsing fails.</param>
    /// <returns><see langword="true" /> if parsing succeeded; otherwise <see langword="false" />.</returns>
    public static bool TryParseResponse(string json, out ApprovalResponse? response)
    {
        try
        {
            response = JsonSerializer.Deserialize(
                json,
                ApprovalJsonContext.Default.ApprovalResponse
            );
            return response is not null;
        }
        catch (JsonException)
        {
            response = null;
            return false;
        }
    }

    /// <summary>
    ///     Builds a human-readable approval message describing the pending basket action.
    /// </summary>
    /// <param name="bookId">Book identifier.</param>
    /// <param name="quantity">Quantity the agent wants to add.</param>
    /// <returns>A confirmation message suitable for display in the approval dialog.</returns>
    public static string BuildBasketApprovalMessage(string bookId, int quantity) =>
        $"The assistant wants to add {quantity} {(quantity == 1 ? "copy" : "copies")} of book {bookId} to your basket. Do you approve?";

    /// <summary>
    ///     Builds a human-readable approval message describing the pending review submission.
    /// </summary>
    /// <param name="bookId">Book identifier.</param>
    /// <param name="rating">Star rating to be submitted.</param>
    /// <returns>A confirmation message suitable for display in the approval dialog.</returns>
    public static string BuildReviewApprovalMessage(string bookId, int rating) =>
        $"The assistant wants to submit a {rating}-star review for book {bookId}. Do you approve?";
}
