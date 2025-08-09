namespace BookWorm.Agent.Sentiment.Extensions;

internal static class Extensions
{
    public static void AddApplicationServices(this IHostApplicationBuilder builder)
    {
        builder.AddDefaultOpenApi();

        builder.Services.AddKernel();

        builder.AddSkTelemetry();

        builder.AddChatCompletion();
    }
}
