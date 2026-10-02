using System.Text.Json;
using System.Text.Json.Serialization;
using BookWorm.Contracts;

namespace BookWorm.Finance.ContractTests;

[JsonSerializable(typeof(UserCheckedOutIntegrationEvent))]
[JsonSerializable(typeof(BasketDeletedCompleteIntegrationEvent))]
[JsonSerializable(typeof(BasketDeletedFailedIntegrationEvent))]
[JsonSerializable(typeof(OrderStatusChangedToCompleteIntegrationEvent))]
[JsonSerializable(typeof(OrderStatusChangedToCancelIntegrationEvent))]
[JsonSerializable(typeof(PlaceOrderCommand))]
[JsonSerializable(typeof(DeleteBasketCompleteCommand))]
[JsonSerializable(typeof(DeleteBasketFailedCommand))]
[JsonSerializable(typeof(CompleteOrderCommand))]
[JsonSerializable(typeof(CancelOrderCommand))]
[JsonSourceGenerationOptions(JsonSerializerDefaults.Web)]
internal sealed partial class PactMessageSerializationContext : JsonSerializerContext;
