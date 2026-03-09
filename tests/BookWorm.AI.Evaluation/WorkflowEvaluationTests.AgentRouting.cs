using BookWorm.AI.Evaluation.Setup;

namespace BookWorm.AI.Evaluation;

public sealed partial class WorkflowEvaluationTests
{
    [Test]
    public async Task GivenFrustratedCustomer_WhenResponding_ThenShowsEmpathy()
    {
        await using var scenarioRun = await s_defaultReportingConfig.CreateScenarioRunAsync(
            "SentimentAnalysis.FrustratedCustomer"
        );

        var chatClient = scenarioRun.ChatConfiguration!.ChatClient;

        var (messages, response) = await TestSetup.GetBookstoreConversationAsync(
            chatClient,
            "I'm really disappointed! I ordered a book two weeks ago and it still hasn't arrived. "
                + "This is the worst experience I've ever had with an online bookstore!"
        );

        var result = await scenarioRun.EvaluateAsync(messages, response);

        ValidateNoWarnings(result);
    }

    [Test]
    public async Task GivenAmbiguousRequest_WhenRouting_ThenClarifiesAndAssists()
    {
        await using var scenarioRun = await s_defaultReportingConfig.CreateScenarioRunAsync(
            "Routing.AmbiguousRequest"
        );

        var chatClient = scenarioRun.ChatConfiguration!.ChatClient;

        var (messages, response) = await TestSetup.GetBookstoreConversationAsync(
            chatClient,
            "I need help with my order"
        );

        var result = await scenarioRun.EvaluateAsync(messages, response);

        ValidateNoWarnings(result);
    }

    [Test]
    public async Task GivenNonEnglishInput_WhenProcessing_ThenHandlesGracefully()
    {
        await using var scenarioRun = await s_defaultReportingConfig.CreateScenarioRunAsync(
            "LanguageTranslation.NonEnglishInput"
        );

        var chatClient = scenarioRun.ChatConfiguration!.ChatClient;

        var (messages, response) = await TestSetup.GetBookstoreConversationAsync(
            chatClient,
            "Tôi muốn tìm sách về lập trình Python cho người mới bắt đầu"
        );

        var result = await scenarioRun.EvaluateAsync(messages, response);

        ValidateNoWarnings(result);
    }

    [Test]
    public async Task GivenOffTopicRequest_WhenRouting_ThenPolitelyDeclines()
    {
        await using var scenarioRun = await s_defaultReportingConfig.CreateScenarioRunAsync(
            "Routing.OffTopicRequest"
        );

        var chatClient = scenarioRun.ChatConfiguration!.ChatClient;

        var (messages, response) = await TestSetup.GetBookstoreConversationAsync(
            chatClient,
            "Can you help me book a flight to Paris?"
        );

        var result = await scenarioRun.EvaluateAsync(messages, response);

        ValidateNoWarnings(result);
    }

    [Test]
    public async Task GivenVerboseMessage_WhenProcessing_ThenHandlesLongInput()
    {
        await using var scenarioRun = await s_defaultReportingConfig.CreateScenarioRunAsync(
            "Summarization.VerboseMessage"
        );

        var chatClient = scenarioRun.ChatConfiguration!.ChatClient;

        var (messages, response) = await TestSetup.GetBookstoreConversationAsync(
            chatClient,
            """
            I've been a customer of your bookstore for many years now and I have to say that overall
            I've had a wonderful experience. However, recently I've been looking for some specific books
            that I can't seem to find anywhere. I'm particularly interested in rare first editions of
            classic literature, especially works by Charles Dickens and Jane Austen. I've also been
            exploring modern literary fiction and would love recommendations similar to "The Goldfinch"
            by Donna Tartt or "A Little Life" by Hanya Yanagihara. Additionally, I'm wondering if you
            offer any book club packages or reading group discounts, as I run a community book club with
            about 15 members and we're always looking for our next great read. Can you help with all of this?
            """
        );

        var result = await scenarioRun.EvaluateAsync(messages, response);

        ValidateNoWarnings(result);
    }
}
