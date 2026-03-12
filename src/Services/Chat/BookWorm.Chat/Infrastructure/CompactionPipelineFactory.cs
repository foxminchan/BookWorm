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

    /// <summary>
    ///     Creates a full 4-layer compaction pipeline for tool-heavy agents.
    ///     <list type="number">
    ///         <item>
    ///             <description>
    ///                 <see cref="ToolResultCompactionStrategy" /> — gentle: collapses older
    ///                 tool-call groups into concise inline summaries.
    ///             </description>
    ///         </item>
    ///         <item>
    ///             <description>
    ///                 <see cref="SummarizationCompactionStrategy" /> — moderate: LLM-summarizes
    ///                 older conversation spans.
    ///             </description>
    ///         </item>
    ///         <item>
    ///             <description>
    ///                 <see cref="SlidingWindowCompactionStrategy" /> — aggressive: retains only
    ///                 the most recent N user turns.
    ///             </description>
    ///         </item>
    ///         <item>
    ///             <description>
    ///                 <see cref="TruncationCompactionStrategy" /> — emergency: hard token-budget
    ///                 backstop.
    ///             </description>
    ///         </item>
    ///     </list>
    /// </summary>
    /// <param name="summarizerClient">
    ///     The <see cref="IChatClient" /> used by the summarization strategy to compress older
    ///     conversation spans. Can be a smaller, cheaper model than the primary agent model.
    /// </param>
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

    /// <summary>
    ///     Creates a lightweight compaction pipeline for agents without tool calls.
    ///     Uses sliding window and truncation only.
    /// </summary>
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
