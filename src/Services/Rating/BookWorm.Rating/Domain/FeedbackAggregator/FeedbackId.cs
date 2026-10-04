using Vogen;

namespace BookWorm.Rating.Domain.FeedbackAggregator;

[ValueObject<Guid>(Conversions.SystemTextJson)]
public readonly partial record struct FeedbackId;
