using Vogen;

namespace BookWorm.Catalog.Domain.AggregatesModel.PublisherAggregate;

[ValueObject<Guid>(Conversions.SystemTextJson)]
public readonly partial record struct PublisherId;
