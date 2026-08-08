namespace BookWorm.AI.Evaluation.Setup;

internal static class EnvironmentVariables
{
    private const string AzureOpenAIEndpointVariableName = "AZURE_OPENAI_ENDPOINT";
    private const string AzureOpenAIApiKeyVariableName = "AZURE_OPENAI_API_KEY";

    public static bool HasAzureOpenAIEndpoint =>
        Environment.GetEnvironmentVariable(AzureOpenAIEndpointVariableName) is not null;

    public static string AzureOpenAIEndpoint =>
        Environment.GetEnvironmentVariable(AzureOpenAIEndpointVariableName)
        ?? throw new InvalidOperationException(
            $"The {AzureOpenAIEndpointVariableName} environment variable is not set."
        );

    public static string? AzureOpenAIApiKey =>
        Environment.GetEnvironmentVariable(AzureOpenAIApiKeyVariableName);

    public static string StorageRootPath => Path.Combine(AppContext.BaseDirectory, "eval-results");
}
