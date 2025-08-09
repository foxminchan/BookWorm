using System.Text.Json;
using System.Text.Json.Serialization;
using ModelContextProtocol.Server;

namespace BookWorm.McpTools.Tools;

[McpServerToolType]
public sealed class Rating(IRatingApi ratingApi)
{
    public async Task<string> GetFeedbackAsync(Guid bookId)
    {
        var feedbacks = await ratingApi.ListRatingsAsync(new() { BookId = bookId });

        return JsonSerializer.Serialize(
            feedbacks.Items,
            FeedbackSerializationContext.Default.IReadOnlyListRating
        );
    }
}

[JsonSerializable(typeof(IReadOnlyList<Rating>))]
[JsonSourceGenerationOptions(
    PropertyNameCaseInsensitive = true,
    PropertyNamingPolicy = JsonKnownNamingPolicy.CamelCase
)]
internal sealed partial class FeedbackSerializationContext : JsonSerializerContext;
