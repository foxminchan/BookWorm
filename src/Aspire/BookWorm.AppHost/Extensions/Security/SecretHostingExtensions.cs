using System.Diagnostics;
using System.Runtime.InteropServices;
using CliWrap;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace BookWorm.AppHost.Extensions.Security;

public static class SecretHostingExtensions
{
    /// <summary>
    ///     Adds an HMAC secret key to the resource builder's environment configuration.
    /// </summary>
    /// <param name="builder">The resource builder to configure.</param>
    /// <returns>The configured <see cref="IResourceBuilder{ProjectResource}" /> instance.</returns>
    public static IResourceBuilder<ProjectResource> WithHmacSecret(
        this IResourceBuilder<ProjectResource> builder
    )
    {
        var hmacKey = builder
            .ApplicationBuilder.AddParameter($"{builder.Resource.Name}-hmac-key", true)
            .WithGeneratedDefault(new() { MinLength = 32, Special = false });

        return builder.WithEnvironment("HMAC__Key", hmacKey);
    }

    /// <summary>
    ///     Configures the resource builder to generate a default parameter value and override the initial state of the
    ///     resource.
    /// </summary>
    /// <param name="builder">
    ///     The resource builder to configure.
    /// </param>
    /// <param name="generateParameterDefault">
    ///     A delegate used to generate the default parameter value.
    /// </param>
    /// <returns>
    ///     The configured <see cref="IResourceBuilder{ParameterResource}" /> instance.
    /// </returns>
    /// <remarks>
    ///     This method generates a default parameter value using the provided delegate and assigns it to the resource.
    ///     It also overrides the initial state of the resource to ensure parameters are hidden by default and properly bound
    ///     to the configuration source. This behavior is adapted from the internal static helper
    ///     <c>AddParameter(this IDistributedApplicationBuilder builder, ...)</c> in
    ///     Aspire.Hosting.ParameterResourceBuilderExtensions.
    /// </remarks>
    public static IResourceBuilder<ParameterResource> WithGeneratedDefault(
        this IResourceBuilder<ParameterResource> builder,
        GenerateParameterDefault generateParameterDefault
    )
    {
        var generatedParameter = ParameterResourceBuilderExtensions.CreateGeneratedParameter(
            builder.ApplicationBuilder,
            builder.Resource.Name,
            builder.Resource.Secret,
            generateParameterDefault
        );

        builder.Resource.Default = generatedParameter.Default;

        builder.WithInitialState(
            new()
            {
                ResourceType = "Parameter",
                IsHidden = true,
                Properties =
                [
                    new("parameter.secret", builder.Resource.Secret.ToString()),
                    new(
                        CustomResourceKnownProperties.Source,
                        $"Parameters:{builder.Resource.Name}"
                    ),
                ],
            }
        );

        return builder;
    }

    /// <summary>
    ///     Injects the ASP.NET Core HTTPS developer certificate into the resource via the specified environment variables when
    ///     <paramref name="builder" />.<see cref="IResourceBuilder{T}.ApplicationBuilder">ApplicationBuilder</see>.
    ///     <see cref="IDistributedApplicationBuilder.ExecutionContext">ExecutionContext</see>.
    ///     <see cref="DistributedApplicationExecutionContext.IsRunMode">IsRunMode</see><c> == true</c>.<br />
    ///     If the resource is a <see cref="ContainerResource" />, the certificate files will be bind mounted into the
    ///     container.
    /// </summary>
    /// <remarks>
    ///     This method <strong>does not</strong> configure an HTTPS endpoint on the resource.
    ///     Use <see cref="ResourceBuilderExtensions.WithHttpsEndpoint{TResource}" /> to configure an HTTPS endpoint.
    /// </remarks>
    public static IResourceBuilder<TResource> RunWithHttpsDevCertificate<TResource>(
        this IResourceBuilder<TResource> builder,
        string certFileEnv,
        string certKeyFileEnv,
        Action<string, string>? onSuccessfulExport = null
    )
        where TResource : IResourceWithEnvironment
    {
        var appBuilder = builder.ApplicationBuilder;

        if (appBuilder.ExecutionContext.IsRunMode && appBuilder.Environment.IsDevelopment())
        {
            appBuilder.Eventing.Subscribe<BeforeStartEvent>(
                async (e, _) =>
                {
                    var logger = e
                        .Services.GetRequiredService<ResourceLoggerService>()
                        .GetLogger(builder.Resource);

                    // Export the ASP.NET Core HTTPS development certificate & private key to files and configure the resource to use them via
                    // the specified environment variables.
                    var (exported, certPath, certKeyPath) = await TryExportDevCertificateAsync(
                        appBuilder,
                        logger
                    );

                    if (!exported)
                    {
                        // The export failed for some reason, don't configure the resource to use the certificate.
                        return;
                    }

                    if (builder.Resource is ContainerResource containerResource)
                    {
                        // Bind-mount the certificate files into the container.
                        const string devCertBindMountDestDir = "/dev-certs";

                        var certFileName = Path.GetFileName(certPath);
                        var certKeyFileName = Path.GetFileName(certKeyPath);

                        var bindSource =
                            Path.GetDirectoryName(certPath) ?? throw new UnreachableException();

                        var certFileDest = $"{devCertBindMountDestDir}/{certFileName}";
                        var certKeyFileDest = $"{devCertBindMountDestDir}/{certKeyFileName}";

                        appBuilder
                            .CreateResourceBuilder(containerResource)
                            .WithBindMount(bindSource, devCertBindMountDestDir, true)
                            .WithEnvironment(certFileEnv, certFileDest)
                            .WithEnvironment(certKeyFileEnv, certKeyFileDest);
                    }
                    else
                    {
                        builder
                            .WithEnvironment(certFileEnv, certPath)
                            .WithEnvironment(certKeyFileEnv, certKeyPath);
                    }

                    onSuccessfulExport?.Invoke(certPath, certKeyPath);
                }
            );
        }

        return builder;
    }

    private static async Task<(
        bool,
        string CertFilePath,
        string CertKeyFilPath
    )> TryExportDevCertificateAsync(IDistributedApplicationBuilder builder, ILogger logger)
    {
        // Exports the ASP.NET Core HTTPS development certificate & private key to PEM files using 'dotnet dev-certs https' to a temporary
        // directory and returns the path.
        // First, try to find existing certificates on platforms that export them to files (e.g., macOS, Linux)
        if (
            RuntimeInformation.IsOSPlatform(OSPlatform.OSX)
            || RuntimeInformation.IsOSPlatform(OSPlatform.Linux)
        )
        {
            var (found, existingCertPath, existingKeyPath) = TryFindExistingDevCertificates(logger);
            if (found)
            {
                return (true, existingCertPath, existingKeyPath);
            }
        }

        var appNameHash = builder.Configuration["AppHost:Sha256"]![..10];
        var tempDir = Path.Combine(Path.GetTempPath(), $"aspire.{appNameHash}");
        var certExportPath = Path.Combine(tempDir, "dev-cert.pem");
        var certKeyExportPath = Path.Combine(tempDir, "dev-cert.key");

        if (File.Exists(certExportPath) && File.Exists(certKeyExportPath))
        {
            // Certificate already exported, return the path.
            logger.LogDebug(
                "Using previously exported dev cert files '{CertPath}' and '{CertKeyPath}'",
                certExportPath,
                certKeyExportPath
            );
            return (true, certExportPath, certKeyExportPath);
        }

        if (File.Exists(certExportPath))
        {
            logger.LogTrace(
                "Deleting previously exported dev cert file '{CertPath}'",
                certExportPath
            );
            File.Delete(certExportPath);
        }

        if (File.Exists(certKeyExportPath))
        {
            logger.LogTrace(
                "Deleting previously exported dev cert key file '{CertKeyPath}'",
                certKeyExportPath
            );
            File.Delete(certKeyExportPath);
        }

        if (!Directory.Exists(tempDir))
        {
            logger.LogTrace("Creating directory to export dev cert to '{ExportDir}'", tempDir);
            Directory.CreateDirectory(tempDir);
        }

        Span<string> args =
        [
            "dev-certs",
            "https",
            "--export-path",
            certExportPath,
            "--format",
            "Pem",
            "--no-password",
        ];

        logger.LogTrace(
            "Running command to export dev cert: dotnet {Args}",
            string.Join(' ', args!)
        );

        try
        {
            using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(5));
            var result = await Cli.Wrap("dotnet")
                .WithArguments([.. args])
                .WithStandardOutputPipe(
                    PipeTarget.ToDelegate(line => logger.LogInformation("> {StandardOutput}", line))
                )
                .WithStandardErrorPipe(
                    PipeTarget.ToDelegate(line => logger.LogError("! {ErrorOutput}", line))
                )
                .WithValidation(CommandResultValidation.None)
                .ExecuteAsync(cts.Token)
                .ConfigureAwait(false);

            if (
                result.ExitCode == 0
                && File.Exists(certExportPath)
                && File.Exists(certKeyExportPath)
            )
            {
                logger.LogDebug(
                    "Dev cert exported to '{CertPath}' and '{CertKeyPath}'",
                    certExportPath,
                    certKeyExportPath
                );
                return (true, certExportPath, certKeyExportPath);
            }

            logger.LogError(
                "HTTPS dev certificate export failed with exit code {ExitCode}",
                result.ExitCode
            );
            return default;
        }
        catch (OperationCanceledException ex)
        {
            logger.LogError(ex, "HTTPS dev certificate export timed out");
            return default;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to export HTTPS dev certificate");
            return default;
        }
    }

    private static (bool Found, string CertPath, string KeyPath) TryFindExistingDevCertificates(
        ILogger logger
    )
    {
        // Try to find existing ASP.NET Core HTTPS development certificates on macOS and Linux
        var homeDir = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
        var httpsDir = Protocol.Https;

        // Common locations where ASP.NET Core dev certificates might be stored
        Span<string> possibleLocations = RuntimeInformation.IsOSPlatform(OSPlatform.OSX)
            ?
            [
                Path.Combine(homeDir, ".aspnet", httpsDir),
                Path.Combine(homeDir, "Library", "Application Support", "ASP.NET", httpsDir),
            ]
            :
            [
                Path.Combine(homeDir, ".aspnet", httpsDir),
                Path.Combine(homeDir, ".dotnet", httpsDir),
            ];

        foreach (var location in possibleLocations)
        {
            if (!Directory.Exists(location))
            {
                continue;
            }

            // Look for .pem certificate files and corresponding .key files
            var certFiles = Directory.GetFiles(location, "*.pem");
            foreach (var certFile in certFiles)
            {
                var certFileName = Path.GetFileNameWithoutExtension(certFile);
                var keyFile = Path.Combine(location, $"{certFileName}.key");

                if (!File.Exists(keyFile))
                {
                    continue;
                }

                logger.LogDebug(
                    "Found existing dev cert files '{CertPath}' and '{KeyPath}'",
                    certFile,
                    keyFile
                );
                return (true, certFile, keyFile);
            }
        }

        logger.LogDebug("No existing dev certificate files found on macOS/Linux");
        return (false, string.Empty, string.Empty);
    }
}
