using BookWorm.ServiceDefaults.ApiSpecification.OpenApi;

namespace BookWorm.Agent.Summarize.Extensions;

internal static class Extensions
{
    public static void AddApplicationServices(this IHostApplicationBuilder builder)
    {
        builder.AddDefaultOpenApi();

        builder.Services.AddKernel();

        builder.AddSkTelemetry();

        builder.AddChatCompletion();
    }

    public static void MapA2AEndpoints(this WebApplication app, string tag)
    {
        var hostAgent = new A2AHostAgent(
            AgentFactory.CreateAgent(app.Services.GetRequiredService<Kernel>()),
            AgentFactory.GetAgentCard()
        );

        app.MapA2A(hostAgent.TaskManager!, "/").WithTags(tag);

        app.MapHttpA2A(hostAgent.TaskManager!, "/").WithTags(tag);
    }
}
