using Microsoft.Extensions.AI;

namespace BookWorm.Chassis.AI.Decisions;

public sealed class DecisionOptions
{
    public string? ModelId { get; set; }

    public Func<IDecisionClient, object?>? RawRepresentationFactory { get; set; }

    public AdditionalPropertiesDictionary? AdditionalProperties { get; set; }

    public DecisionOptions Clone()
    {
        return new()
        {
            ModelId = ModelId,
            RawRepresentationFactory = RawRepresentationFactory,
            AdditionalProperties = AdditionalProperties is null
                ? null
                : new AdditionalPropertiesDictionary(AdditionalProperties),
        };
    }
}

public sealed class DecisionClientMetadata(
    string? providerName = null,
    Uri? providerUri = null,
    string? defaultModelId = null,
    IReadOnlyList<DecisionKind>? supportedKinds = null
)
{
    public string? ProviderName { get; } = providerName;

    public Uri? ProviderUri { get; } = providerUri;

    public string? DefaultModelId { get; } = defaultModelId;

    public IReadOnlyList<DecisionKind>? SupportedKinds { get; } = supportedKinds;
}

public sealed class DecisionClientException : Exception
{
    public DecisionClientException() { }

    public DecisionClientException(string? message, bool isTransient = false)
        : base(message)
    {
        IsTransient = isTransient;
    }

    public DecisionClientException(
        string? message,
        Exception? innerException,
        bool isTransient = false
    )
        : base(message, innerException)
    {
        IsTransient = isTransient;
    }

    public bool IsTransient { get; }
}
