using Microsoft.Agents.AI.Compaction;
using Microsoft.Extensions.AI;

namespace BookWorm.Rating.Infrastructure.Agents;

internal static class RatingCompactionPipelineFactory
{
    private const int ToolResultMessageThreshold = 20;
    private const int SummarizationTokenThreshold = 3_000;
    private const int SlidingWindowTurnThreshold = 12;
    private const int TruncationTokenBudget = 24_000;

    public static CompactionProvider Create(IChatClient summarizerClient)
    {
        PipelineCompactionStrategy pipeline = new(
            new ToolResultCompactionStrategy(
                CompactionTriggers.MessagesExceed(ToolResultMessageThreshold)
            ),
            new SummarizationCompactionStrategy(
                summarizerClient,
                CompactionTriggers.TokensExceed(SummarizationTokenThreshold)
            ),
            new SlidingWindowCompactionStrategy(
                CompactionTriggers.TurnsExceed(SlidingWindowTurnThreshold)
            ),
            new TruncationCompactionStrategy(CompactionTriggers.TokensExceed(TruncationTokenBudget))
        );

        return new(pipeline);
    }
}
