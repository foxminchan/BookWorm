namespace BookWorm.Chassis.AI.Decisions.Abstractions.Answers;

public sealed class DecisionProbability
{
    public DecisionProbability(string id, double value)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(id);
        DecisionAnswer.ValidateProbability(value, nameof(value));

        Id = id;
        Value = value;
    }

    public string Id { get; }

    public double Value { get; }
}
