using System.Diagnostics;
using System.Runtime.InteropServices;
using CliWrap;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace BookWorm.AppHost.Extensions.Security;

public static class DevCertHostingExtensions
{
    /// <summary>
    ///     Configures a resource to use the ASP.NET Core HTTPS development certificate for secure connections.
    /// </summary>
    /// <typeparam name="TResource">The type of resource that implements <see cref="IResourceWithEnvironment" />.</typeparam>
    /// <param name="builder">The resource builder to configure with HTTPS development certificate support.</param>
    /// <param name="certFileEnv">The environment variable name for the certificate file path.</param>
    /// <param name="certKeyFileEnv">The environment variable name for the certificate key file path.</param>
    /// <param name="onSuccessfulExport">
    ///     Optional callback invoked when the certificate is successfully exported, receiving the
    ///     certificate and key file paths.
    /// </param>
    /// <returns>The configured resource builder with HTTPS development certificate integration.</returns>
    /// <remarks>
    ///     This method provides comprehensive HTTPS development certificate management:
    ///     - <strong>Development mode only:</strong> Only applies configuration when running in development environment
    ///     - <strong>Automatic certificate export:</strong> Uses <c>dotnet dev-certs https</c> command to export certificates
    ///     to PEM format
    ///     - <strong>Platform-specific handling:</strong> Attempts to find existing certificates on macOS and Linux before
    ///     exporting
    ///     - <strong>Container support:</strong> Creates bind mounts to <c>/dev-certs</c> directory for container resources
    ///     - <strong>Non-container support:</strong> Sets environment variables with local file paths for non-container
    ///     resources
    ///     - <strong>Caching mechanism:</strong> Reuses previously exported certificates to avoid unnecessary exports
    ///     - <strong>Error handling:</strong> Gracefully handles export failures and timeouts with comprehensive logging
    ///     - <strong>Cross-platform compatibility:</strong> Works on Windows, macOS, and Linux with platform-specific
    ///     optimizations
    /// </remarks>
    /// <example>
    ///     <code>
    ///     builder.AddContainer("api", "my-api-image")
    ///            .RunWithHttpsDevCertificate("HTTPS_CERT_FILE", "HTTPS_CERT_KEY_FILE");
    ///
    ///     // With success callback
    ///     builder.AddContainer("web", "my-web-image")
    ///            .RunWithHttpsDevCertificate("CERT_PATH", "KEY_PATH",
    ///                (certPath, keyPath) => Console.WriteLine($"Cert: {certPath}, Key: {keyPath}"));
    ///     </code>
    /// </example>
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
