using System.Text.Json;
using System.Text.Json.Serialization;
using BookWorm.Contracts;

namespace BookWorm.Finance.Features.Orders;

[JsonSerializable(typeof(UserCheckedOutIntegrationEvent))]
[JsonSerializable(typeof(BasketDeletedCompleteIntegrationEvent))]
[JsonSerializable(typeof(BasketDeletedFailedIntegrationEvent))]
[JsonSerializable(typeof(OrderStatusChangedToCancelIntegrationEvent))]
[JsonSerializable(typeof(OrderStatusChangedToCompleteIntegrationEvent))]
[JsonSourceGenerationOptions(JsonSerializerDefaults.Web)]
internal sealed partial class FinanceEventSerializationContext : JsonSerializerContext;
