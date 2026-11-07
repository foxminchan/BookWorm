using Vogen;

namespace BookWorm.Notification.Domain.Models;

[ValueObject<Guid>(Conversions.SystemTextJson)]
public readonly partial record struct OrderId;
