using Microsoft.Extensions.AI;

namespace BookWorm.Chassis.AI.Decisions.Abstractions;

public sealed class DecisionOptions
{
    public string? ModelId { get; private init; }

    private Func<IDecisionClient, object?>? RawRepresentationFactory { get; init; }

    private AdditionalPropertiesDictionary? AdditionalProperties { get; init; }

    public DecisionOptions Clone()
    {
        return new()
        {
            ModelId = ModelId,
            RawRepresentationFactory = RawRepresentationFactory,
            AdditionalProperties = AdditionalProperties is null
                ? null
                : new AdditionalPropertiesDictionary([]),
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

public sealed class DecisionClientException(
    string? message,
    Exception? innerException,
    bool isTransient = false
) : Exception(message, innerException)
{
    public bool IsTransient { get; } = isTransient;
}
