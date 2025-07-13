using System.Diagnostics;
using System.IO.Hashing;
using System.Text;
using CliWrap;

namespace BookWorm.AppHost.Extensions.Security;

public static class HostingExtensions
{
    private const string AspNet = "aspnet";
    private const string Dotnet = "dotnet";
    private const string DevCertDir = "dev-certs";
    private const string DevCertPem = "dev-cert.pem";
    private const string DevCertKey = "dev-cert.key";

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
    ///     This method <strong>does not</strong> configure an HTTPS endpoint on the resource. Use
    ///     <see cref="ResourceBuilderExtensions.WithHttpsEndpoint{TResource}" /> to configure an HTTPS endpoint.
    /// </remarks>
    public static IResourceBuilder<TResource> RunWithHttpsDevCertificate<TResource>(
        this IResourceBuilder<TResource> builder,
        string certFileEnv,
        string certKeyFileEnv,
        Action<string, string>? onSuccessfulExport = null
    )
        where TResource : IResourceWithEnvironment
    {
        var applicationBuilder = builder.ApplicationBuilder;

        if (!applicationBuilder.ExecutionContext.IsRunMode)
        {
            return builder;
        }

        // Export the ASP.NET Core HTTPS development certificate and private key to PEM files, bind-mount them into the container
        // and configure it to use them via the specified environment variables.
        var (certPath, keyPath) = ExportDevCertificateSync(applicationBuilder);

        var bindSource = Path.GetDirectoryName(certPath) ?? throw new UnreachableException();

        if (builder.Resource is ContainerResource containerResource)
        {
            applicationBuilder
                .CreateResourceBuilder(containerResource)
                .WithBindMount(bindSource, $"/{DevCertDir}", true);
        }

        builder
            .WithEnvironment(certFileEnv, $"/{DevCertDir}/{DevCertPem}")
            .WithEnvironment(certKeyFileEnv, $"/{DevCertDir}/{DevCertKey}");

        onSuccessfulExport?.Invoke(certPath, keyPath);

        return builder;
    }

    private static (string, string) ExportDevCertificateSync(IDistributedApplicationBuilder builder)
    {
        // Synchronous wrapper that uses Task.Run to avoid blocking the caller's synchronization context
        return Task.Run(async () => await ExportDevCertificate(builder).ConfigureAwait(false))
            .GetAwaiter()
            .GetResult();
    }

    private static async Task<(string, string)> ExportDevCertificate(
        IDistributedApplicationBuilder builder
    )
    {
        // Exports the ASP.NET Core HTTPS development certificate & private key to PEM files using 'dotnet dev-certs https' to a temporary
        // directory and returns the path.
        var appNameHashBytes = XxHash64.Hash(
            Encoding.Unicode.GetBytes(builder.Environment.ApplicationName).AsSpan()
        );
        var appNameHash = Convert.ToHexStringLower(appNameHashBytes);
        var tempDir = Path.Combine(
            Path.GetTempPath(),
            $"{nameof(Aspire).ToLowerInvariant()}.{appNameHash}"
        );
        var certExportPath = Path.Combine(tempDir, DevCertPem);
        var certKeyExportPath = Path.Combine(tempDir, DevCertKey);

        // Check if certificates already exist in the temp directory
        if (File.Exists(certExportPath) && File.Exists(certKeyExportPath))
        {
            return (certExportPath, certKeyExportPath);
        }

        // Check for platform-specific certificate locations
        if (OperatingSystem.IsMacOS())
        {
            var (macCertPath, macKeyPath) = await TryGetMacOsCertificate(tempDir);
            if (macCertPath is not null && macKeyPath is not null)
            {
                return (macCertPath, macKeyPath);
            }
        }
        else if (OperatingSystem.IsLinux())
        {
            var (linuxCertPath, linuxKeyPath) = await TryGetLinuxCertificate(tempDir);
            if (linuxCertPath is not null && linuxKeyPath is not null)
            {
                return (linuxCertPath, linuxKeyPath);
            }
        }

        // Fallback to exporting certificates using dotnet dev-certs
        try
        {
            if (Directory.Exists(tempDir))
            {
                Directory.Delete(tempDir, true);
            }

            Directory.CreateDirectory(tempDir);
        }
        catch (IOException ex)
        {
            throw new InvalidOperationException(
                $"Failed to prepare certificate directory: {ex.Message}",
                ex
            );
        }

        Directory.CreateDirectory(tempDir);

        var result = await Cli.Wrap(Dotnet)
            .WithArguments(
                [
                    DevCertDir,
                    Protocol.Https,
                    "--export-path",
                    certExportPath,
                    "--format",
                    "Pem",
                    "--no-password",
                ]
            )
            .WithValidation(CommandResultValidation.None)
            .ExecuteAsync(CancellationToken.None)
            .ConfigureAwait(false);

        if (result.ExitCode == 0 && File.Exists(certExportPath) && File.Exists(certKeyExportPath))
        {
            return (certExportPath, certKeyExportPath);
        }

        throw new InvalidOperationException(
            $"HTTPS dev certificate export failed with exit code {result.ExitCode}"
        );
    }

    private static async Task<(string?, string?)> TryGetMacOsCertificate(string tempDir)
    {
        // On macOS, ASP.NET Core dev certificates are stored in the Keychain
        if (!await IsCertificateAvailable().ConfigureAwait(false))
        {
            return (null, null);
        }

        // Try to find existing PEM files in common locations
        var homeDir = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
        var aspnetHttpsDir = Path.Combine(homeDir, $".{AspNet}", Protocol.Https);

        return await TryFindAndCopyCertificates(tempDir, [aspnetHttpsDir]).ConfigureAwait(false);
    }

    private static async Task<(string?, string?)> TryGetLinuxCertificate(string tempDir)
    {
        // On Linux, check if certificates are available via dotnet dev-certs
        if (!await IsCertificateAvailable().ConfigureAwait(false))
        {
            return (null, null);
        }

        // Try to find existing certificates in common Linux locations
        var homeDir = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
        var aspnetHttpsDir = Path.Combine(homeDir, $".{AspNet}", Protocol.Https);
        var dotnetToolsDir = Path.Combine(
            homeDir,
            $".{Dotnet}",
            "corefx",
            "cryptography",
            "x509stores"
        );

        return await TryFindAndCopyCertificates(tempDir, [aspnetHttpsDir, dotnetToolsDir])
            .ConfigureAwait(false);
    }

    private static async Task<bool> IsCertificateAvailable()
    {
        var checkResult = await Cli.Wrap(Dotnet)
            .WithArguments([DevCertDir, Protocol.Https, "--check", "--verbose"])
            .WithValidation(CommandResultValidation.None)
            .ExecuteAsync()
            .ConfigureAwait(false);

        return checkResult.ExitCode == 0;
    }

    private static Task<(string?, string?)> TryFindAndCopyCertificates(
        string tempDir,
        string[] searchDirectories
    )
    {
        var searchDirs = searchDirectories.Where(Directory.Exists);

        foreach (var searchDir in searchDirs)
        {
            var pemFiles = Directory.GetFiles(searchDir, "*.pem", SearchOption.AllDirectories);
            var keyFiles = Directory.GetFiles(searchDir, "*.key", SearchOption.AllDirectories);

            if (pemFiles.Length <= 0 || keyFiles.Length <= 0)
            {
                continue;
            }

            var certPath = Path.Combine(tempDir, DevCertPem);
            var keyPath = Path.Combine(tempDir, DevCertKey);

            Directory.CreateDirectory(tempDir);
            File.Copy(pemFiles[0], certPath, true);
            File.Copy(keyFiles[0], keyPath, true);

            return Task.FromResult<(string?, string?)>((certPath, keyPath));
        }

        return Task.FromResult<(string?, string?)>((null, null));
    }
}
