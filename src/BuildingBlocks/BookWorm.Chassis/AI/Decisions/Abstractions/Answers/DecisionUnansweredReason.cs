namespace BookWorm.Chassis.AI.Decisions.Abstractions.Answers;

public readonly record struct DecisionUnansweredReason
{
    public DecisionUnansweredReason(string value)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(value);
        Value = value;
    }

    public static DecisionUnansweredReason Refusal { get; } = new("refusal");

    public string Value { get; }

    public override string ToString()
    {
        return Value;
    }
}
