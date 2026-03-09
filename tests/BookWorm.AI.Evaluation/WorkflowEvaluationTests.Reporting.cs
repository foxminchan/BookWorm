using BookWorm.AI.Evaluation.Setup;
using Microsoft.Extensions.AI.Evaluation.Reporting;
using Microsoft.Extensions.AI.Evaluation.Reporting.Formats.Html;
using Microsoft.Extensions.AI.Evaluation.Reporting.Storage;

namespace BookWorm.AI.Evaluation;

public sealed partial class WorkflowEvaluationTests
{
    [Test]
    [DependsOn(nameof(GivenSciFiQuery_WhenSearchingBooks_ThenReturnsRelevantResults))]
    [DependsOn(
        nameof(GivenFantasyPreference_WhenRecommendingBooks_ThenResponseIsGroundedInCatalog)
    )]
    [DependsOn(nameof(GivenReturnPolicyQuestion_WhenResponding_ThenReturnsAccurateDetails))]
    [DependsOn(nameof(GivenShippingInquiry_WhenResponding_ThenReturnsAccurateInfo))]
    [DependsOn(nameof(GivenLoyaltyProgramQuestion_WhenResponding_ThenReturnsAccurateDetails))]
    [DependsOn(nameof(GivenFrustratedCustomer_WhenResponding_ThenShowsEmpathy))]
    [DependsOn(nameof(GivenAmbiguousRequest_WhenRouting_ThenClarifiesAndAssists))]
    [DependsOn(nameof(GivenNonEnglishInput_WhenProcessing_ThenHandlesGracefully))]
    [DependsOn(nameof(GivenOffTopicRequest_WhenRouting_ThenPolitelyDeclines))]
    [DependsOn(nameof(GivenVerboseMessage_WhenProcessing_ThenHandlesLongInput))]
    public async Task GivenAllEvaluationsComplete_WhenGeneratingReport_ThenProducesHtmlReport()
    {
        var results = new List<ScenarioRunResult>();
        var resultStore = new DiskBasedResultStore(EnvironmentVariables.StorageRootPath);

        await foreach (var executionName in resultStore.GetLatestExecutionNamesAsync(1))
        {
            await foreach (var result in resultStore.ReadResultsAsync(executionName))
            {
                results.Add(result);
            }
        }

        var reportPath = Path.Combine(EnvironmentVariables.StorageRootPath, "report.html");
        var reportWriter = new HtmlReportWriter(reportPath);

        await reportWriter.WriteReportAsync(results);
    }
}
