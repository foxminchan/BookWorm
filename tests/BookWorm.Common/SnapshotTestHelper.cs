using System.Runtime.CompilerServices;
using System.Text.RegularExpressions;

namespace BookWorm.Common;

public static partial class SnapshotTestHelper
{
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

                builder.Clear();
                builder.Append(scrubbedContent);
            })
            .UseSnapshotDirectory(sourceFilePath);
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
}
