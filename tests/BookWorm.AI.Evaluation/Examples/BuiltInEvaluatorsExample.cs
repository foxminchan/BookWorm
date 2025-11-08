namespace BookWorm.AI.Evaluation.Examples;

/// <summary>
/// Example demonstrating how to use built-in evaluators from Microsoft.Extensions.AI.Evaluation.Quality.
/// 
/// NOTE: These examples require an actual IChatClient implementation to work.
/// For testing, you would need to either:
/// 1. Configure a real chat client (e.g., Azure OpenAI, OpenAI)
/// 2. Mock the IChatClient interface to return expected scores
/// 
/// This file serves as documentation and reference for using AI-assisted evaluators.
/// </summary>
public static class BuiltInEvaluatorsExample
{
    /// <summary>
    /// Example: Using CoherenceEvaluator to evaluate response readability.
    /// 
    /// CoherenceEvaluator measures how readable and user-friendly the response is.
    /// Requires an LLM judge (GPT-4o recommended) to score the response.
    /// Returns a score between 1 (poor) and 5 (excellent).
    /// </summary>
    public static async Task<EvaluationResult> EvaluateCoherenceAsync(
        IEnumerable<ChatMessage> messages,
        ChatResponse response,
        IChatClient judgeClient)
    {
        // Create the evaluator
        IEvaluator coherenceEvaluator = new CoherenceEvaluator();

        // Configure the chat client that will act as the judge
        var chatConfiguration = new ChatConfiguration(judgeClient);

        // Evaluate the response
        EvaluationResult result = await coherenceEvaluator.EvaluateAsync(
            messages,
            response,
            chatConfiguration);

        // Extract the coherence metric
        NumericMetric coherence = result.Get<NumericMetric>(CoherenceEvaluator.CoherenceMetricName);

        // Example interpretation:
        // coherence.Value: 1-5 (higher is better)
        // coherence.Interpretation.Rating: Unknown, Unacceptable, Poor, Average, Good, Exceptional
        // coherence.Interpretation.Reason: Explanation of the score

        return result;
    }

    /// <summary>
    /// Example: Using RelevanceEvaluator to evaluate response relevance to the query.
    /// 
    /// RelevanceEvaluator measures how well the response addresses the user's query.
    /// Requires an LLM judge (GPT-4o recommended) to score the response.
    /// Returns a score between 1 (poor) and 5 (excellent).
    /// </summary>
    public static async Task<EvaluationResult> EvaluateRelevanceAsync(
        IEnumerable<ChatMessage> messages,
        ChatResponse response,
        IChatClient judgeClient)
    {
        // Create the evaluator
        IEvaluator relevanceEvaluator = new RelevanceEvaluator();

        // Configure the chat client that will act as the judge
        var chatConfiguration = new ChatConfiguration(judgeClient);

        // Evaluate the response
        EvaluationResult result = await relevanceEvaluator.EvaluateAsync(
            messages,
            response,
            chatConfiguration);

        // Extract the relevance metric
        NumericMetric relevance = result.Get<NumericMetric>(RelevanceEvaluator.RelevanceMetricName);

        return result;
    }

    /// <summary>
    /// Example: Using CompletenessEvaluator with ground truth to evaluate response completeness.
    /// 
    /// CompletenessEvaluator measures how thoroughly the response addresses all aspects
    /// compared to a ground truth reference.
    /// Requires an LLM judge (GPT-4o recommended) and ground truth data.
    /// Returns a score between 1 (poor) and 5 (excellent).
    /// </summary>
    public static async Task<EvaluationResult> EvaluateCompletenessAsync(
        IEnumerable<ChatMessage> messages,
        ChatResponse response,
        string groundTruth,
        IChatClient judgeClient)
    {
        // Create the evaluator
        IEvaluator completenessEvaluator = new CompletenessEvaluator();

        // Configure the chat client that will act as the judge
        var chatConfiguration = new ChatConfiguration(judgeClient);

        // Create evaluation context with ground truth
        var context = new CompletenessEvaluatorContext(groundTruth);

        // Evaluate the response
        EvaluationResult result = await completenessEvaluator.EvaluateAsync(
            messages,
            response,
            chatConfiguration,
            [context]);

        // Extract the completeness metric
        NumericMetric completeness = result.Get<NumericMetric>(CompletenessEvaluator.CompletenessMetricName);

        return result;
    }

    /// <summary>
    /// Example: Using FluencyEvaluator to evaluate response naturalness.
    /// 
    /// FluencyEvaluator measures how natural and fluent the language in the response is.
    /// Requires an LLM judge (GPT-4o recommended) to score the response.
    /// Returns a score between 1 (poor) and 5 (excellent).
    /// </summary>
    public static async Task<EvaluationResult> EvaluateFluencyAsync(
        IEnumerable<ChatMessage> messages,
        ChatResponse response,
        IChatClient judgeClient)
    {
        // Create the evaluator
        IEvaluator fluencyEvaluator = new FluencyEvaluator();

        // Configure the chat client that will act as the judge
        var chatConfiguration = new ChatConfiguration(judgeClient);

        // Evaluate the response
        EvaluationResult result = await fluencyEvaluator.EvaluateAsync(
            messages,
            response,
            chatConfiguration);

        // Extract the fluency metric
        NumericMetric fluency = result.Get<NumericMetric>(FluencyEvaluator.FluencyMetricName);

        return result;
    }

    /// <summary>
    /// Example: Combining multiple evaluators for comprehensive evaluation.
    /// 
    /// This demonstrates how to run multiple evaluators and aggregate results.
    /// </summary>
    public static async Task<Dictionary<string, EvaluationResult>> EvaluateComprehensivelyAsync(
        IEnumerable<ChatMessage> messages,
        ChatResponse response,
        IChatClient judgeClient)
    {
        var results = new Dictionary<string, EvaluationResult>();

        // Create multiple evaluators
        IEvaluator[] evaluators =
        [
            new CoherenceEvaluator(),
            new RelevanceEvaluator(),
            new FluencyEvaluator()
        ];

        // Configure the chat client
        var chatConfiguration = new ChatConfiguration(judgeClient);

        // Evaluate with each evaluator
        foreach (var evaluator in evaluators)
        {
            var result = await evaluator.EvaluateAsync(
                messages,
                response,
                chatConfiguration);

            foreach (var metricName in evaluator.EvaluationMetricNames)
            {
                results[metricName] = result;
            }
        }

        return results;
    }

    /// <summary>
    /// Example: Best practices for setting up Azure OpenAI as the judge.
    /// 
    /// This is pseudocode showing how to configure Azure OpenAI for evaluation.
    /// Actual implementation would require Azure.AI.OpenAI and related packages.
    /// </summary>
    public static IChatClient CreateAzureOpenAIJudgeClient()
    {
        // Pseudocode - actual implementation would be:
        /*
        var endpoint = new Uri(Environment.GetEnvironmentVariable("AZURE_OPENAI_ENDPOINT")!);
        var credential = new AzureKeyCredential(Environment.GetEnvironmentVariable("AZURE_OPENAI_API_KEY")!);
        var deploymentName = "gpt-4o"; // Use GPT-4o for best results
        
        var openAIClient = new AzureOpenAIClient(endpoint, credential);
        IChatClient chatClient = openAIClient.AsChatClient(deploymentName);
        
        return chatClient;
        */

        throw new NotImplementedException(
            "Configure Azure OpenAI client with your credentials. " +
            "See Microsoft.Extensions.AI.OpenAI documentation.");
    }
}
