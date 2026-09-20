using Vogen;

namespace BookWorm.Rating.Domain.FeedbackAggregator;

[ValueObject<Guid>(conversions: Conversions.SystemTextJson | Conversions.EfCoreValueConverter)]
public readonly partial record struct FeedbackId;
