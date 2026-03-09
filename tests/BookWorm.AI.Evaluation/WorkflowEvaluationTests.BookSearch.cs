using BookWorm.AI.Evaluation.Setup;
using Microsoft.Extensions.AI.Evaluation.Quality;

namespace BookWorm.AI.Evaluation;

public sealed partial class WorkflowEvaluationTests
{
    [Test]
    public async Task GivenSciFiQuery_WhenSearchingBooks_ThenReturnsRelevantResults()
    {
        await using var scenarioRun = await s_defaultReportingConfig.CreateScenarioRunAsync(
            "BookSearch.RelevantResults"
        );

        var chatClient = scenarioRun.ChatConfiguration!.ChatClient;

        var (messages, response) = await TestSetup.GetBookstoreConversationAsync(
            chatClient,
            "I'm looking for science fiction books by Isaac Asimov. Can you recommend some?"
        );

        var result = await scenarioRun.EvaluateAsync(messages, response);

        ValidateNoWarnings(result);
    }

    [Test]
    public async Task GivenFantasyPreference_WhenRecommendingBooks_ThenResponseIsGroundedInCatalog()
    {
        await using var scenarioRun = await s_groundednessReportingConfig.CreateScenarioRunAsync(
            "BookSearch.GroundedRecommendation"
        );

        var chatClient = scenarioRun.ChatConfiguration!.ChatClient;

        var (messages, response) = await TestSetup.GetBookstoreConversationAsync(
            chatClient,
            "What fantasy books do you have for someone who loved The Lord of the Rings?"
        );

        var result = await scenarioRun.EvaluateAsync(
            messages,
            response,
            [
                new GroundednessEvaluatorContext(
                    """
                    BookWorm catalog includes: "The Name of the Wind" by Patrick Rothfuss (epic fantasy, $14.99),
                    "A Game of Thrones" by George R.R. Martin (epic fantasy, $16.99),
                    "The Way of Kings" by Brandon Sanderson (epic fantasy, $18.99),
                    "The Wheel of Time: Eye of the World" by Robert Jordan (epic fantasy, $15.99),
                    "The Hobbit" by J.R.R. Tolkien (fantasy, $12.99).
                    All items are in stock with standard and express shipping available.
                    """
                ),
            ]
        );

        ValidateNoWarnings(result);
    }
}
