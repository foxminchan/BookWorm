namespace BookWorm.AI.Evaluation.Setup;

[AttributeUsage(AttributeTargets.Assembly | AttributeTargets.Class | AttributeTargets.Method)]
public sealed class RequiresOpenAIKeyAttribute()
    : SkipAttribute("AZURE_OPENAI_ENDPOINT environment variable is not set.")
{
    public override Task<bool> ShouldSkip(TestRegisteredContext context)
    {
        return Task.FromResult(!EnvironmentVariables.HasAzureOpenAIEndpoint);
    }
}
