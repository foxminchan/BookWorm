using Aspire.Hosting.Pipelines;
using CliWrap;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace BookWorm.AppHost.Extensions.Infrastructure;

public static class PipelineExtensions
{
    public static void AddGhcrPushStep(this IDistributedApplicationPipeline pipeline)
    {
        pipeline.AddStep(
            "push-gh",
            async context =>
            {
                var configuration = context.Services.GetRequiredService<IConfiguration>();

                var tag = configuration["CONTAINER_TAG"] ?? "latest";
                var ghcrOrganization =
                    configuration["GHCR_ORGANIZATION"]
                    ?? throw new InvalidOperationException(
                        "No GHCR_ORGANIZATION found in environment variables"
                    );
                var ghcrRepository =
                    configuration["GHCR_REPOSITORY"]
                    ?? throw new InvalidOperationException(
                        "No GHCR_REPOSITORY found in environment variables"
                    );

                var resources = context.Model.Resources.OfType<ProjectResource>().ToList();

                if (resources.Count == 0)
                {
                    context.Logger.LogInformation(
                        "No Resources of type ProjectResource found in the model. Skipping GHCR push step."
                    );
                    return;
                }

                foreach (var resource in resources)
                {
                    var resourcePath = resource.GetProjectMetadata().ProjectPath;

                    var containerRepository =
                        $"{ghcrOrganization}/{ghcrRepository}/{resource.Name}";

                    context.Logger.LogInformation(
                        "Pushing Docker image {ImageName} to GHCR...",
                        containerRepository
                    );

                    var command = Cli.Wrap("dotnet")
                        .WithArguments(args =>
                            args.Add("publish")
                                .Add(resourcePath)
                                .Add("-t:PublishContainer")
                                .Add("--verbosity")
                                .Add("quiet")
                                .Add("--nologo")
                                .Add("-r")
                                .Add("linux-x64")
                                .Add("-p:ContainerRegistry=ghcr.io")
                                .Add($"-p:ContainerRepository={containerRepository}")
                                .Add($"-p:ContainerImageTag={tag}")
                        )
                        .WithStandardOutputPipe(
                            PipeTarget.ToDelegate(line =>
                                context.Logger.LogInformation("{OutputLine}", line)
                            )
                        )
                        .WithStandardErrorPipe(
                            PipeTarget.ToDelegate(line =>
                                context.Logger.LogError("{ErrorLine}", line)
                            )
                        );

                    await command.ExecuteAsync();
                }
            },
            dependsOn: WellKnownPipelineSteps.Build
        );
    }
}
