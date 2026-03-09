using BookWorm.AI.Evaluation.Evaluators;
using BookWorm.AI.Evaluation.Setup;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.AI.Evaluation;
using Microsoft.Extensions.AI.Evaluation.Quality;
using Microsoft.Extensions.AI.Evaluation.Reporting;
using Microsoft.Extensions.AI.Evaluation.Reporting.Storage;

namespace BookWorm.AI.Evaluation;

[RequiresOpenAIKey]
public sealed partial class WorkflowEvaluationTests : IAsyncDisposable
{
    private static readonly string s_executionName = $"{DateTime.Now:yyyyMMddTHHmmss}";

    private static readonly ChatConfiguration s_chatConfiguration =
        TestSetup.GetChatConfiguration();

    private static readonly ReportingConfiguration s_defaultReportingConfig =
        DiskBasedReportingConfiguration.Create(
            EnvironmentVariables.StorageRootPath,
            GetDefaultEvaluators(),
            s_chatConfiguration,
            true,
            executionName: s_executionName,
            tags: GetTags()
        );

    private static readonly ReportingConfiguration s_groundednessReportingConfig =
        DiskBasedReportingConfiguration.Create(
            EnvironmentVariables.StorageRootPath,
            GetGroundednessEvaluators(),
            s_chatConfiguration,
            true,
            executionName: s_executionName,
            tags: GetTags()
        );

    public ValueTask DisposeAsync()
    {
        return ValueTask.CompletedTask;
    }

    private static IEnumerable<IEvaluator> GetDefaultEvaluators()
    {
        yield return new RelevanceEvaluator();
        yield return new CoherenceEvaluator();
        yield return new FluencyEvaluator();
        yield return new CompletenessEvaluator();
        yield return new DomainRelevanceEvaluator();
        yield return new ResponseLengthEvaluator();
    }

    private static IEnumerable<IEvaluator> GetGroundednessEvaluators()
    {
        yield return new RelevanceEvaluator();
        yield return new GroundednessEvaluator();
        yield return new CoherenceEvaluator();
        yield return new FluencyEvaluator();
        yield return new CompletenessEvaluator();
        yield return new DomainRelevanceEvaluator();
        yield return new ResponseLengthEvaluator();
    }

    private static IEnumerable<string> GetTags()
    {
        yield return $"Execution: {s_executionName}";

        var metadata = s_chatConfiguration.ChatClient.GetService<ChatClientMetadata>();

        yield return $"Provider: {metadata?.ProviderName ?? "Unknown"}";
        yield return $"Model: {metadata?.DefaultModelId ?? "Unknown"}";
    }

    private static void ValidateNoWarnings(EvaluationResult result)
    {
        result
            .ContainsDiagnostics(d => d.Severity >= EvaluationDiagnosticSeverity.Warning)
            .ShouldBeFalse();
    }
}
