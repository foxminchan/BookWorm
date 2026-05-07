using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.RegularExpressions;
using BookWorm.Chassis.EventBus;
using Wolverine.Attributes;

namespace BookWorm.Common;

public static partial class SnapshotTestHelper
{
    private static readonly JsonSerializerOptions _webJsonOptions = new(JsonSerializerDefaults.Web)
    {
        WriteIndented = true,
        Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
    };

    /// <summary>
    ///     Verifies the JSON representation of an object against a snapshot, scrubbing out variable data
    ///     like GUIDs and DateTime values to ensure deterministic snapshots. Use this for simple objects
    ///     or collections that don't need to be wrapped in a CloudEvents envelope. For messages that
    ///     represent integration events or commands, prefer the VerifyCloudEvent(s) methods to
    ///     assert the exact wire format consumed by downstream services.
    /// </summary>
    /// <param name="target">The object to serialize to JSON and verify as a snapshot.</param>
    /// <param name="sourceFilePath">
    ///     The caller file path is used to determine the snapshot directory. It is automatically
    ///     populated by the compiler, so you don't need to provide it when calling this method.
    /// </param>
    public static Task Verify(object target, [CallerFilePath] string sourceFilePath = "")
    {
        return Verifier
            .Verify(target)
            .UseStrictJson()
            .AddScrubber(builder =>
            {
                var content = builder.ToString();

                // Scrub GUIDs to make snapshots deterministic
                var scrubbedContent = GuidRegex().Replace(content, "Guid_Scrubbed");

                // Scrub DateTime values to make snapshots deterministic
                scrubbedContent = DateTimeRegex().Replace(scrubbedContent, "DateTime_Scrubbed");

                // Scrub W3C Trace Context IDs (e.g., 00-<trace-id>-<span-id>-<flags>)
                scrubbedContent = TraceIdRegex().Replace(scrubbedContent, "TraceId_Scrubbed");

                builder.Clear();
                builder.Append(scrubbedContent);
            })
            .UseSnapshotDirectory(sourceFilePath);
    }

    /// <summary>
    ///     Wraps a single integration event or command in a CloudEvents 1.0 envelope shape
    ///     and verifies it as a snapshot. Use this in publisher and consumer contract tests to
    ///     assert the exact wire format consumed by downstream services.
    /// </summary>
    /// <param name="message">The message to wrap and verify.</param>
    /// <param name="sourceFilePath">
    ///     The caller file path is used to determine the snapshot directory. It is automatically
    ///     populated by the compiler, so you don't need to provide it when calling this method.
    /// </param>
    public static Task VerifyCloudEvent(object message, [CallerFilePath] string sourceFilePath = "")
    {
        return VerifyJson(BuildCloudEventJson(message), sourceFilePath);
    }

    /// <summary>
    ///     Wraps each message in a collection in a CloudEvents 1.0 envelope shape and verifies
    ///     them as a snapshot. Use this when a handler publishes multiple outgoing messages via
    /// </summary>
    /// <param name="messages">The collection of messages to wrap and verify.</param>
    /// <param name="sourceFilePath">
    ///     The caller file path is used to determine the snapshot directory. It is automatically
    ///     populated by the compiler, so you don't need to provide it when calling this method.
    /// </param>
    public static Task VerifyCloudEvents(
        IEnumerable<object?> messages,
        [CallerFilePath] string sourceFilePath = ""
    )
    {
        var envelopes = messages
            .Where(m => m is not null)
            .Select(m => JsonSerializer.Deserialize<JsonElement>(BuildCloudEventJson(m!)))
            .ToList();
        return VerifyJson(JsonSerializer.Serialize(envelopes, _webJsonOptions), sourceFilePath);
    }

    private static string BuildCloudEventJson(object message)
    {
        var type = message.GetType();
        var messageType =
            type.GetCustomAttribute<MessageIdentityAttribute>()?.Alias
            ?? type.FullName
            ?? type.Name;

        var assemblyName = type.Assembly.GetName().Name ?? string.Empty;
        var source = $"urn:bookworm:{ToKebabCase(assemblyName)}";

        Guid id;
        DateTimeOffset time;
        if (message is IntegrationEvent ie)
        {
            id = ie.Id;
            time = new(ie.CreationDate, TimeSpan.Zero);
        }
        else
        {
            id = Guid.NewGuid();
            time = DateTimeOffset.UtcNow;
        }

        // Serialize data with camelCase naming. Reparsing to JsonElement ensures STJ
        // writes the value inline (not as a struct) when the envelope is serialized.
        var data = JsonSerializer.Deserialize<JsonElement>(
            JsonSerializer.Serialize(message, type, _webJsonOptions)
        );

        return JsonSerializer.Serialize(
            new
            {
                specversion = "1.0",
                id,
                type = messageType,
                source,
                time,
                datacontenttype = "application/json",
                data,
            },
            _webJsonOptions
        );
    }

    private static Task VerifyJson(string json, string sourceFilePath)
    {
        return Verifier
            .Verify(json, "json")
            .AddScrubber(builder =>
            {
                var content = builder.ToString();
                var scrubbedContent = GuidRegex().Replace(content, "Guid_Scrubbed");
                scrubbedContent = DateTimeRegex().Replace(scrubbedContent, "DateTime_Scrubbed");
                scrubbedContent = TraceIdRegex().Replace(scrubbedContent, "TraceId_Scrubbed");
                builder.Clear();
                builder.Append(scrubbedContent);
            })
            .UseSnapshotDirectory(sourceFilePath);
    }

    private static string ToKebabCase(string input)
    {
        return KebabCaseRegex().Replace(input.Replace(".", "-"), "-$1").ToLowerInvariant();
    }

    private static SettingsTask UseSnapshotDirectory(
        this SettingsTask settingsTask,
        string sourceFilePath
    )
    {
        // If we have a caller file path, start from that directory
        var startDirectory = !string.IsNullOrWhiteSpace(sourceFilePath)
            ? Path.GetDirectoryName(sourceFilePath) ?? Directory.GetCurrentDirectory()
            : Directory.GetCurrentDirectory();

        var projectRoot =
            FindTestProjectRoot(startDirectory)
            ?? throw new DirectoryNotFoundException(
                "Could not locate test project root directory. Please ensure the test project file (.csproj) exists."
            );

        var snapshotsDirectory = Path.Combine(projectRoot, "Snapshots");
        return settingsTask.UseDirectory(snapshotsDirectory);
    }

    private static string? FindTestProjectRoot(string startPath)
    {
        var directory = new DirectoryInfo(startPath);

        while (directory is not null)
        {
            // Look for .csproj files that contain "Tests" in the name
            var csprojFiles = directory.GetFiles("*.csproj");
            if (
                csprojFiles.Length > 0
                && csprojFiles.Any(f =>
                    f.Name.Contains("Tests", StringComparison.OrdinalIgnoreCase)
                )
            )
            {
                return directory.FullName;
            }

            directory = directory.Parent;
        }

        return null;
    }

    [GeneratedRegex(
        @"\b[0-9a-fA-F]{8}-[0-9a-fA-F]{4}-[0-9a-fA-F]{4}-[0-9a-fA-F]{4}-[0-9a-fA-F]{12}\b"
    )]
    private static partial Regex GuidRegex();

    [GeneratedRegex(@"\d{4}-\d{2}-\d{2}T\d{2}:\d{2}:\d{2}(?:\.\d{1,7})?(?:Z|[+-]\d{2}:\d{2})")]
    private static partial Regex DateTimeRegex();

    [GeneratedRegex(@"\b[0-9a-fA-F]{2}-[0-9a-fA-F]{32}-[0-9a-fA-F]{16}-[0-9a-fA-F]{2}\b")]
    private static partial Regex TraceIdRegex();

    [GeneratedRegex(@"(?<=[a-z0-9])([A-Z])")]
    private static partial Regex KebabCaseRegex();
}
