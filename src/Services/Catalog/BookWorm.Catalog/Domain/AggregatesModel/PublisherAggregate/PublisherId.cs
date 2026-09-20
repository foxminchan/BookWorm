using Vogen;

namespace BookWorm.Catalog.Domain.AggregatesModel.PublisherAggregate;

[ValueObject<Guid>(conversions: Conversions.SystemTextJson | Conversions.EfCoreValueConverter)]
public readonly partial record struct PublisherId;
