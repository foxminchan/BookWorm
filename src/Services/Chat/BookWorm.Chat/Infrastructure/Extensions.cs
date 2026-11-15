using BookWorm.Chassis.AI.Presidio;
using BookWorm.Chat.Domain.Services;
using BookWorm.Chat.Infrastructure.Governance;

namespace BookWorm.Chat.Infrastructure;

internal static class Extensions
{
    extension(IHostApplicationBuilder builder)
    {
        public void AddAIInfrastructure()
        {
            var services = builder.Services;

            builder.AddAIProject().WithLlmModels().WithDecisionModels().WithAITelemetry();
            builder.AddPresidio();
            builder.AddMcpClient(Services.McpTools);

            services.AddSingleton<IPromptSafetyEvaluator, GovernancePromptSafetyEvaluator>();
            services.AddSingleton<PromptValidationService>();

            services.AddAGUIServer();
        }
    }
}
