using Vogen;

namespace BookWorm.Catalog.Domain.AggregatesModel.PublisherAggregate;

[ValueObject<Guid>(Conversions.SystemTextJson | Conversions.EfCoreValueConverter)]
public readonly partial record struct PublisherId;
