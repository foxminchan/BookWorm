using System.Text.Json;

namespace BookWorm.Chat.Infrastructure.HitlMiddleware;

/// <summary>
///     Represents an outbound HITL approval request sent from the server to the frontend.
/// </summary>
/// <param name="ApprovalId">Unique call ID correlating request and response.</param>
/// <param name="FunctionName">The name of the function awaiting approval.</param>
/// <param name="FunctionArguments">The serialized arguments for the pending function call.</param>
/// <param name="Message">Human-readable description shown in the approval dialog.</param>
internal sealed record ApprovalRequest(
    string ApprovalId,
    string FunctionName,
    JsonElement FunctionArguments,
    string Message
);

/// <summary>
///     Represents an inbound HITL approval response returned from the frontend to the server.
/// </summary>
/// <param name="ApprovalId">The call ID matching the original <see cref="ApprovalRequest" />.</param>
/// <param name="Approved">Whether the customer approved the pending action.</param>
internal sealed record ApprovalResponse(string ApprovalId, bool Approved);
