using Microsoft.Agents.AI.Compaction;

namespace BookWorm.Chat.Infrastructure;

internal static class CompactionPipelineFactory
{
    // Thresholds tuned for production workloads
    private const int ToolResultMessageThreshold = 30;
    private const int SummarizationTokenThreshold = 4_000;
    private const int SlidingWindowTurnThreshold = 20;
    private const int TruncationTokenBudget = 32_000;

    // Lighter thresholds for agents without tool calls
    private const int LightSlidingWindowTurnThreshold = 15;
    private const int LightTruncationTokenBudget = 16_000;

    public static CompactionProvider CreateFull(IChatClient summarizerClient)
    {
        PipelineCompactionStrategy pipeline = new(
            // 1. Gentle: collapse old tool-call groups into short summaries
            new ToolResultCompactionStrategy(
                CompactionTriggers.MessagesExceed(ToolResultMessageThreshold)
            ),
            // 2. Moderate: LLM-summarize older conversation spans
            new SummarizationCompactionStrategy(
                summarizerClient,
                CompactionTriggers.TokensExceed(SummarizationTokenThreshold)
            ),
            // 3. Aggressive: keep only the last N user turns and their responses
            new SlidingWindowCompactionStrategy(
                CompactionTriggers.TurnsExceed(SlidingWindowTurnThreshold)
            ),
            // 4. Emergency: drop the oldest groups until under the token budget
            new TruncationCompactionStrategy(CompactionTriggers.TokensExceed(TruncationTokenBudget))
        );

        return new(pipeline);
    }

    public static CompactionProvider CreateLight()
    {
        PipelineCompactionStrategy pipeline = new(
            // 1. Aggressive: keep only recent user turns
            new SlidingWindowCompactionStrategy(
                CompactionTriggers.TurnsExceed(LightSlidingWindowTurnThreshold)
            ),
            // 2. Emergency: hard token-budget backstop
            new TruncationCompactionStrategy(
                CompactionTriggers.TokensExceed(LightTruncationTokenBudget)
            )
        );

        return new(pipeline);
    }
}
