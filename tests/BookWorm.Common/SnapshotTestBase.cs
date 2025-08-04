using System.Text.RegularExpressions;

namespace BookWorm.Common;

public abstract partial class SnapshotTestBase
{
    static SnapshotTestBase()
    {
        VerifierSettings.UseStrictJson();

        VerifierSettings.AddScrubber(builder =>
        {
            var content = builder.ToString();

            // Scrub GUIDs to make snapshots deterministic
            var scrubbedContent = GuidRegex().Replace(content, "Guid_Scrubbed");

            // Scrub DateTime values to make snapshots deterministic
            scrubbedContent = DateTimeRegex().Replace(scrubbedContent, "DateTime_Scrubbed");

            builder.Clear();
            builder.Append(scrubbedContent);
        });
    }

    protected static SettingsTask VerifySnapshot(object target)
    {
        return Verify(target).UseDirectory(GetSnapshotsDirectory());
    }

    private static string GetSnapshotsDirectory()
    {
        // Start from the current directory and traverse up to find the test project root
        var currentDirectory = Directory.GetCurrentDirectory();

        var projectRoot =
            FindTestProjectRoot(currentDirectory)
            ?? throw new DirectoryNotFoundException(
                "Could not locate test project root directory. Please ensure the test project file (.csproj) exists."
            );

        return Path.Combine(projectRoot, "Snapshots");
    }

    private static string? FindTestProjectRoot(string startPath)
    {
        var directory = new DirectoryInfo(startPath);

        while (directory is not null)
        {
            // Look for .csproj files that contain "Test" in the name
            var csprojFiles = directory.GetFiles("*.csproj");
            if (
                csprojFiles.Length > 0
                && csprojFiles.Any(f => f.Name.Contains("Test", StringComparison.OrdinalIgnoreCase))
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
