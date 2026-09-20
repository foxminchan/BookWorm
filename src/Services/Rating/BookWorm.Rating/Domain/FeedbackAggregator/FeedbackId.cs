using Vogen;

namespace BookWorm.Rating.Domain.FeedbackAggregator;

[ValueObject<Guid>(Conversions.SystemTextJson | Conversions.EfCoreValueConverter)]
public readonly partial record struct FeedbackId;
