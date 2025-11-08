using BookWorm.AI.Evaluation.Evaluators;

namespace BookWorm.AI.Evaluation.Tests;

/// <summary>
/// Tests for evaluating AI chat responses using built-in and custom evaluators.
/// </summary>
public sealed class ChatResponseEvaluationTests
{
    private static readonly ChatMessage[] SampleMessages =
    [
        new(ChatRole.System, "You are a helpful book recommendation assistant."),
        new(ChatRole.User, "Can you recommend a science fiction book about space exploration?")
    ];

    private static readonly ChatResponse SampleResponse = new(
    [
        new ChatMessage(
            ChatRole.Assistant,
            "I recommend 'The Martian' by Andy Weir. This gripping science fiction novel " +
            "tells the story of astronaut Mark Watney, who becomes stranded on Mars after " +
            "his crew believes him dead during a fierce storm. The book combines hard science " +
            "with compelling storytelling, making it both educational and entertaining. " +
            "It's perfect for fans of space exploration and survival stories.")
    ]);

    [Test]
    public async Task GivenBookResponse_WhenEvaluated_ThenContainsBookInformation()
    {
        // Arrange
        IEvaluator evaluator = new BookRelevanceEvaluator();

        // Act
        EvaluationResult result = await evaluator.EvaluateAsync(
            SampleMessages,
            SampleResponse);

        // Assert
        BooleanMetric metric = result.Get<BooleanMetric>(BookRelevanceEvaluator.BookRelevanceMetricName);
        metric.Value.HasValue.ShouldBeTrue();
        metric.Value!.Value.ShouldBeTrue();
        metric.Interpretation.ShouldNotBeNull();
        metric.Interpretation.Failed.ShouldBeFalse();
        metric.Interpretation.Rating.ShouldBe(EvaluationRating.Good);
        metric.ContainsDiagnostics().ShouldBeFalse();
    }

    [Test]
    public async Task GivenCompleteResponse_WhenEvaluated_ThenMeetsCompletenessThreshold()
    {
        // Arrange
        IEvaluator evaluator = new ResponseCompletenessEvaluator(minimumCharacters: 100, maximumCharacters: 1000);

        // Act
        EvaluationResult result = await evaluator.EvaluateAsync(
            SampleMessages,
            SampleResponse);

        // Assert
        NumericMetric metric = result.Get<NumericMetric>(ResponseCompletenessEvaluator.CompletenessMetricName);
        metric.Value.ShouldNotBeNull();
        metric.Value!.Value.ShouldBeGreaterThanOrEqualTo(4.0);
        metric.Interpretation.ShouldNotBeNull();
        metric.Interpretation.Failed.ShouldBeFalse();
        metric.ContainsDiagnostics().ShouldBeFalse();
    }

    [Test]
    public async Task GivenResponse_WhenEvaluatedWithMultipleEvaluators_ThenAllMetricsAreComputed()
    {
        // Arrange
        IEvaluator[] evaluators =
        [
            new BookRelevanceEvaluator(),
            new ResponseCompletenessEvaluator(minimumCharacters: 100, maximumCharacters: 1000)
        ];

        // Act & Assert
        foreach (var evaluator in evaluators)
        {
            EvaluationResult result = await evaluator.EvaluateAsync(
                SampleMessages,
                SampleResponse);

            result.ShouldNotBeNull();
            result.Metrics.ShouldNotBeEmpty();

            foreach (var metricName in evaluator.EvaluationMetricNames)
            {
                result.Metrics.Keys.ShouldContain(metricName);
            }
        }
    }

    [Test]
    public async Task GivenEmptyResponse_WhenEvaluated_ThenFailsValidation()
    {
        // Arrange
        var emptyResponse = new ChatResponse([new ChatMessage(ChatRole.Assistant, "")]);
        IEvaluator evaluator = new BookRelevanceEvaluator();

        // Act
        EvaluationResult result = await evaluator.EvaluateAsync(
            SampleMessages,
            emptyResponse);

        // Assert
        BooleanMetric metric = result.Get<BooleanMetric>(BookRelevanceEvaluator.BookRelevanceMetricName);
        metric.Value.HasValue.ShouldBeTrue();
        metric.Value!.Value.ShouldBeFalse();
        metric.Interpretation.ShouldNotBeNull();
        metric.Interpretation.Failed.ShouldBeTrue();
    }

    [Test]
    public async Task GivenShortResponse_WhenEvaluated_ThenReceivesLowCompletenessScore()
    {
        // Arrange
        var shortResponse = new ChatResponse([new ChatMessage(ChatRole.Assistant, "Read The Martian.")]);
        IEvaluator evaluator = new ResponseCompletenessEvaluator(minimumCharacters: 100, maximumCharacters: 1000);

        // Act
        EvaluationResult result = await evaluator.EvaluateAsync(
            SampleMessages,
            shortResponse);

        // Assert
        NumericMetric metric = result.Get<NumericMetric>(ResponseCompletenessEvaluator.CompletenessMetricName);
        metric.Value.ShouldNotBeNull();
        metric.Value!.Value.ShouldBeLessThan(4.0);
        metric.Interpretation.ShouldNotBeNull();
    }
}
