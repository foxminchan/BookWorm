using Vogen;

namespace BookWorm.McpTools.Models;

[ValueObject<Guid>(Conversions.SystemTextJson)]
public readonly partial record struct PublisherId;
