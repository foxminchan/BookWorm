using BookWorm.Chassis.AI.Middlewares;
using BookWorm.Chat.Configurations;
using BookWorm.Constants.Other;
using Microsoft.Agents.AI;
using Microsoft.Agents.AI.Hosting;
using Microsoft.Extensions.Options;

namespace BookWorm.Chat.Orchestration.Loop;

internal static class LoopAgentRegistration
{
    private static readonly string[] _qualityCriteria =
    [
        "The response directly addresses every part of the user's request.",
        "Book claims and recommendations are grounded in available catalog tool results.",
        "The response is concise, actionable, and does not invent unavailable books.",
    ];

    extension(IHostApplicationBuilder builder)
    {
        public void AddLoopAgent()
        {
            builder
                .Services.AddOptions<LoopEngineeringOptions>()
                .BindConfiguration(LoopEngineeringOptions.ConfigurationSection)
                .Validate(
                    options => options.MaxIterations is >= 1 and <= 10,
                    "Chat loop MaxIterations must be between 1 and 10."
                )
                .Validate(
                    options =>
                        options.ExecutionTimeout >= TimeSpan.FromSeconds(5)
                        && options.ExecutionTimeout <= TimeSpan.FromMinutes(10),
                    "Chat loop ExecutionTimeout must be between 5 seconds and 10 minutes."
                )
                .ValidateOnStart();

            builder
                .AddAIAgent(
                    LoopAgentDefinition.Name,
                    (sp, _) =>
                    {
                        var options = sp.GetRequiredService<
                            IOptions<LoopEngineeringOptions>
                        >().Value;
                        var workflowAgent = sp.GetRequiredKeyedService<AIAgent>(Workflows.Chat);
                        var judgeClient = sp.GetRequiredService<IChatClient>()
                            .AsBuilder()
                            .UsePIIMiddleware(sp)
                            .UseGuardrailMiddleware()
                            .Build(sp);

                        return CreateLoopAgent(
                            workflowAgent,
                            judgeClient,
                            options,
                            sp.GetService<ILoggerFactory>()
                        );
                    }
                )
                .WithInMemorySessionStore();
        }
    }

    internal static BoundedLoopAgent CreateLoopAgent(
        AIAgent workflowAgent,
        IChatClient judgeClient,
        LoopEngineeringOptions options,
        ILoggerFactory? loggerFactory = null
    )
    {
        ArgumentNullException.ThrowIfNull(workflowAgent);
        ArgumentNullException.ThrowIfNull(judgeClient);
        ArgumentNullException.ThrowIfNull(options);

        var evaluator = new AIJudgeLoopEvaluator(
            judgeClient,
            new AIJudgeLoopEvaluatorOptions
            {
                Criteria = _qualityCriteria,
                FeedbackMessageTemplate =
                    "Close the following gaps, using tools when needed, then return a revised final answer:\n{gap_analysis}",
            }
        );
        var loopAgent = new LoopAgent(
            workflowAgent,
            evaluator,
            new LoopAgentOptions
            {
                MaxIterations = options.MaxIterations,
                ExcludeOnBehalfOfMessages = true,
                NonStreamingReturnsLastResponseOnly = true,
                OnBehalfOfAuthorName = LoopAgentDefinition.Name,
            },
            loggerFactory
        );

        return new(loopAgent, options.ExecutionTimeout);
    }
}
