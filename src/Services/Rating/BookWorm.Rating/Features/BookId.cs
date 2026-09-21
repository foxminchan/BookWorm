using Vogen;

namespace BookWorm.Rating.Features;

[ValueObject<Guid>(Conversions.SystemTextJson)]
public readonly partial record struct BookId;
