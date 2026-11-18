namespace BookWorm.AppHost;

internal static class RepositoryPaths
{
    private const string SolutionFileName = "BookWorm.slnx";
    private static readonly Lazy<string> _repositoryRoot = new(FindRepositoryRoot);

    public static string ServiceProjectPath(string service, string? project = null)
    {
        project ??= $"{nameof(BookWorm)}.{service}";

        return Path.Combine(
            _repositoryRoot.Value,
            "src",
            "Services",
            service,
            project,
            $"{project}.csproj"
        );
    }

    private static string FindRepositoryRoot()
    {
        var directory = new DirectoryInfo(AppContext.BaseDirectory);

        while (directory is not null)
        {
            if (File.Exists(Path.Combine(directory.FullName, SolutionFileName)))
            {
                return directory.FullName;
            }

            directory = directory.Parent;
        }

        throw new DirectoryNotFoundException("Could not locate the BookWorm repository root.");
    }
}
