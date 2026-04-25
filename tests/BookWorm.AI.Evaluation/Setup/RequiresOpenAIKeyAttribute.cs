namespace BookWorm.AI.Evaluation.Setup;

[AttributeUsage(AttributeTargets.Assembly | AttributeTargets.Class | AttributeTargets.Method)]
public sealed class RequiresOpenAIKeyAttribute()
    : SkipAttribute("Neither OPENAI_API_KEY nor OPENAI_BASE_URL environment variable is set.")
{
    public override Task<bool> ShouldSkip(TestRegisteredContext context)
    {
        return Task.FromResult(
            !EnvironmentVariables.HasOpenAIApiKey && !EnvironmentVariables.IsAIMockMode
        );
    }
}
