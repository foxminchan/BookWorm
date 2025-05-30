﻿using Aspire.Hosting.Yarp;

namespace BookWorm.AppHost.Extensions;

public static class K6Extensions
{
    /// <summary>
    ///     Adds K6 load testing to the distributed application.
    /// </summary>
    /// <param name="builder">The distributed application builder to configure.</param>
    /// <param name="entryPoint">The resource builder for the project resource to test.</param>
    /// <remarks>
    ///     This method configures a K6 load testing instance that:
    ///     - Mounts scripts from a Container/scripts directory
    ///     - Mounts reports to Container/reports directory
    ///     - Runs the main.js script with a random number of virtual users (10-100)
    ///     - References and waits for the specified entryPoint resource
    ///     - Only runs in run mode, not in publish mode
    /// </remarks>
    public static void AddK6(
        this IDistributedApplicationBuilder builder,
        IResourceBuilder<YarpResource> entryPoint
    )
    {
        if (builder.ExecutionContext.IsRunMode)
        {
            builder
                .AddK6(Components.K6)
                .WithImagePullPolicy(ImagePullPolicy.Always)
                .WithBindMount("Container/k6", "/scripts", true)
                .WithBindMount("Container/k6/dist", "/home/k6")
                .WithScript("/scripts/dist/main.js")
                .WithReference(entryPoint.Resource.GetEndpoint("http"))
                .WithEnvironment("K6_WEB_DASHBOARD", "true")
                .WithEnvironment("K6_WEB_DASHBOARD_EXPORT", "dashboard-report.html")
                .WithHttpEndpoint(
                    targetPort: Components.K6Dashboard.ContainerPort,
                    name: Components.K6Dashboard.Name
                )
                .WithUrlForEndpoint(
                    Components.K6Dashboard.Name,
                    url => url.DisplayText = "K6 Dashboard"
                )
                .WaitFor(entryPoint);
        }
    }
}
