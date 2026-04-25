namespace BookWorm.AI.Evaluation.Setup;

internal static class EnvironmentVariables
{
    private const string OpenAIApiKeyVariableName = "OPENAI_API_KEY";
    private const string OpenAIBaseUrlVariableName = "OPENAI_BASE_URL";

    public static bool HasOpenAIApiKey =>
        Environment.GetEnvironmentVariable(OpenAIApiKeyVariableName) is not null;

    public static bool IsAIMockMode =>
        Environment.GetEnvironmentVariable(OpenAIBaseUrlVariableName) is not null;

    public static string OpenAIApiKey =>
        Environment.GetEnvironmentVariable(OpenAIApiKeyVariableName) ?? "mock-key";

    public static string? OpenAIBaseUrl =>
        Environment.GetEnvironmentVariable(OpenAIBaseUrlVariableName);

    public static string StorageRootPath => Path.Combine(AppContext.BaseDirectory, "eval-results");
}
