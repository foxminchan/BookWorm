namespace BookWorm.AI.Evaluation.Setup;

internal static class EnvironmentVariables
{
    private const string OpenAIApiKeyVariableName = "OPENAI_API_KEY";

    public static bool HasOpenAIApiKey =>
        Environment.GetEnvironmentVariable(OpenAIApiKeyVariableName) is not null;

    public static string OpenAIApiKey =>
        Environment.GetEnvironmentVariable(OpenAIApiKeyVariableName)
        ?? throw new InvalidOperationException(
            $"Set the {OpenAIApiKeyVariableName} environment variable to run evaluations."
        );

    public static string StorageRootPath => Path.Combine(AppContext.BaseDirectory, "eval-results");
}
