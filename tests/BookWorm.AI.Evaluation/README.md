# BookWorm AI Model Evaluation

This project contains AI model evaluation tests using Microsoft.Extensions.AI.Evaluation framework.

## Overview

The evaluation framework helps assess the quality of AI model responses in the BookWorm application. It includes both built-in evaluators from Microsoft and custom evaluators specific to the BookWorm domain.

## Evaluators

### Built-in Evaluators

The project uses the following built-in evaluators from Microsoft.Extensions.AI.Evaluation.Quality:

- **CoherenceEvaluator**: Measures the readability and user-friendliness of responses (score 1-5)
- **RelevanceEvaluator**: Evaluates how relevant the response is to the user's query (score 1-5)
- **CompletenessEvaluator**: Assesses how thoroughly the response addresses the query (score 1-5)
- **FluentEvaluator**: Measures the naturalness and fluency of the language (score 1-5)

### Custom Evaluators

#### BookRelevanceEvaluator

Evaluates whether a response contains book-related information. Returns a boolean metric indicating if the response includes keywords like "book", "author", "title", etc.

**Usage:**
```csharp
IEvaluator evaluator = new BookRelevanceEvaluator();
EvaluationResult result = await evaluator.EvaluateAsync(messages, response);
BooleanMetric metric = result.Get<BooleanMetric>(BookRelevanceEvaluator.BookRelevanceMetricName);
```

#### ResponseCompletenessEvaluator

Evaluates the response length and completeness. Scores responses based on character count with configurable minimum and maximum thresholds.

**Usage:**
```csharp
IEvaluator evaluator = new ResponseCompletenessEvaluator(
    minimumCharacters: 100, 
    maximumCharacters: 1000);
EvaluationResult result = await evaluator.EvaluateAsync(messages, response);
NumericMetric metric = result.Get<NumericMetric>(ResponseCompletenessEvaluator.CompletenessMetricName);
```

## Running Tests

The evaluation tests are built using TUnit and can be run using:

```bash
dotnet test tests/BookWorm.AI.Evaluation
```

## Evaluation Metrics

Each evaluator returns one or more metrics:

- **BooleanMetric**: Pass/fail evaluation (true/false)
- **NumericMetric**: Scored evaluation (typically 1-5 scale)

All metrics include:
- **Value**: The metric score
- **Reason**: Explanation of the score
- **Interpretation**: Rating (Unknown, Unacceptable, Adequate, Good, Exceptional)

## Configuration

For AI-assisted evaluators (CoherenceEvaluator, RelevanceEvaluator), you need to provide a ChatConfiguration with an IChatClient implementation that connects to an LLM (like GPT-4).

Example:
```csharp
var chatConfiguration = new ChatConfiguration
{
    ChatClient = yourChatClient
};

EvaluationResult result = await evaluator.EvaluateAsync(
    messages, 
    response, 
    chatConfiguration);
```

## Best Practices

1. **Use Multiple Evaluators**: Combine multiple evaluators to get a comprehensive assessment
2. **Mock for Testing**: Use Moq to mock IChatClient for unit testing AI-assisted evaluators
3. **Set Appropriate Thresholds**: Customize evaluator parameters based on your use case
4. **Monitor Metrics**: Track evaluation metrics over time to detect quality regression

## Integration with CI/CD

These evaluation tests can be integrated into your CI/CD pipeline to ensure AI response quality remains consistent:

```bash
# Run as part of your test suite
dotnet test --filter "FullyQualifiedName~BookWorm.AI.Evaluation"
```

## References

- [Microsoft.Extensions.AI.Evaluation Documentation](https://learn.microsoft.com/en-us/dotnet/api/microsoft.extensions.ai.evaluation)
- [Microsoft.Extensions.AI.Evaluation.Quality](https://learn.microsoft.com/en-us/dotnet/api/microsoft.extensions.ai.evaluation.quality)

## Examples

The `Examples/` directory contains code examples demonstrating how to use the built-in evaluators:

- **BuiltInEvaluatorsExample.cs**: Shows how to use CoherenceEvaluator, RelevanceEvaluator, CompletenessEvaluator, and FluencyEvaluator with an LLM judge.

These examples require an actual IChatClient implementation (e.g., Azure OpenAI, OpenAI) to work in production.
