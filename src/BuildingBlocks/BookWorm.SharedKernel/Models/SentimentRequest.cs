namespace BookWorm.SharedKernel.Models;

public sealed class SentimentRequest
{
    public string TextToEvaluate { get; set; } = string.Empty;
}
