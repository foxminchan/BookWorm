namespace BookWorm.AppHost.Extensions.Infrastructure;

internal static partial class AzureExtensions
{
    extension(IResourceBuilder<AzureFunctionsProjectResource> builder)
    {
        public IResourceBuilder<AzureFunctionsProjectResource> WithDurableTaskHub(
            string schedulerName,
            string taskHubName
        )
        {
            if (builder.ApplicationBuilder.ExecutionContext.IsRunMode)
            {
                var durableTaskScheduler = builder
                    .ApplicationBuilder.AddDurableTaskScheduler(schedulerName)
                    .RunAsEmulator(emulator => emulator.WithLifetime(ContainerLifetime.Persistent))
                    .InGroup(ResourceGroups.Functions);

                var taskHub = durableTaskScheduler.AddTaskHub(taskHubName);

                return builder.WithReference(taskHub).WaitFor(taskHub);
            }

            var durableBicep = builder
                .ApplicationBuilder.AddBicepTemplate(
                    schedulerName,
                    "Infra/durable-task-scheduler.bicep"
                )
                .WithParameter("schedulerName", schedulerName)
                .WithParameter("taskHubName", taskHubName)
                .InGroup(ResourceGroups.Functions);

            return builder
                .WithEnvironment(
                    "DTS_CONNECTION_STRING",
                    durableBicep.GetOutput("connectionString")
                )
                .WithEnvironment("TASKHUB_NAME", durableBicep.GetOutput("taskHubName"));
        }
    }
}
