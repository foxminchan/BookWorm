using BookWorm.Rating.Infrastructure.Summarizer;
using ZiggyCreatures.Caching.Fusion;

namespace BookWorm.Rating.Features.Summarize;

public sealed record SummarizeFeedbackQuery(Guid BookId) : IQuery<SummarizeResult>;

internal sealed class SummarizeFeedbackHandler(ISummarizer summarizer, IFusionCache cache)
    : IQueryHandler<SummarizeFeedbackQuery, SummarizeResult>
{
    public async ValueTask<SummarizeResult> Handle(
        SummarizeFeedbackQuery request,
        CancellationToken cancellationToken
    )
    {
        var cacheKey = $"rating:summary:v1:{request.BookId}";

        return await cache.GetOrSetAsync<SummarizeResult>(
            cacheKey,
            async (ctx, ct) =>
            {
                var result = await summarizer.SummarizeAsync(request.BookId, ct);

                // No reviews means no LLM call was made, so keep it short-lived to surface the first reviews quickly.
                if (result.ReviewCount != 0)
                {
                    return result;
                }

                ctx.Options.Duration = TimeSpan.FromMinutes(1);
                ctx.Options.DistributedCacheDuration = TimeSpan.FromMinutes(1);

                return result;
            },
            new FusionCacheEntryOptions
            {
                Duration = TimeSpan.FromHours(6),
                DistributedCacheDuration = TimeSpan.FromHours(6),
            },
            cancellationToken
        );
    }
}
