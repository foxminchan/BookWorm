using System.Text.Json.Serialization;

namespace BookWorm.Chat.Infrastructure.HitlMiddleware;

/// <summary>
///     Source-generated JSON serialization context for HITL approval payloads.
///     Uses snake_case property naming to match the AG-UI protocol convention.
/// </summary>
[JsonSerializable(typeof(ApprovalRequest))]
[JsonSerializable(typeof(ApprovalResponse))]
[JsonSourceGenerationOptions(
    PropertyNamingPolicy = JsonKnownNamingPolicy.SnakeCaseLower,
    WriteIndented = false
)]
internal sealed partial class ApprovalJsonContext : JsonSerializerContext;
