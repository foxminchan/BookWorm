namespace BookWorm.Testing.Pact.Configuration;

internal static class PactDirectoryResolver
{
    private const string OutputDirectoryEnvVar = "BOOKWORM_PACT_OUTPUT_DIRECTORY";
    private const string SolutionFileName = "BookWorm.slnx";

    public static string GetPactDirectory()
    {
        var repositoryRoot = FindRepositoryRoot();
        var configuredDirectory = Environment.GetEnvironmentVariable(OutputDirectoryEnvVar);

        return configuredDirectory is null
            ? Path.Combine(repositoryRoot, "tests", "pacts")
            : Path.GetFullPath(Path.Combine(repositoryRoot, configuredDirectory));
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

        throw new DirectoryNotFoundException(
            "Could not locate the BookWorm repository root for Pact output."
        );
    }
}
