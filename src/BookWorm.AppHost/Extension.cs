namespace BookWorm.AppHost;

internal static class Extension
{
    /// <summary>
    /// Adds a hook to set the ASPNETCORE_FORWARDEDHEADERS_ENABLED environment variable to true for all projects in the application.
    /// </summary>
    public static IDistributedApplicationBuilder AddForwardedHeaders(
        this IDistributedApplicationBuilder builder
    )
    {
        builder.Services.TryAddLifecycleHook<AddForwardHeadersHook>();
        return builder;
    }

    internal sealed class AddForwardHeadersHook : IDistributedApplicationLifecycleHook
    {
        public Task BeforeStartAsync(
            DistributedApplicationModel appModel,
            CancellationToken cancellationToken = default
        )
        {
            foreach (var p in appModel.GetProjectResources())
            {
                p.Annotations.Add(
                    new EnvironmentCallbackAnnotation(context =>
                    {
                        context.EnvironmentVariables["ASPNETCORE_FORWARDEDHEADERS_ENABLED"] =
                            "true";
                    })
                );
            }

            return Task.CompletedTask;
        }
    }
}
