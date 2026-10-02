using System.Text.Json;
using System.Text.Json.Serialization;
using BookWorm.Contracts;

namespace BookWorm.Ordering.ContractTests;

[JsonSerializable(typeof(UserCheckedOutIntegrationEvent))]
[JsonSerializable(typeof(OrderStatusChangedToCompleteIntegrationEvent))]
[JsonSerializable(typeof(OrderStatusChangedToCancelIntegrationEvent))]
[JsonSerializable(typeof(DeleteBasketCompleteCommand))]
[JsonSerializable(typeof(DeleteBasketFailedCommand))]
[JsonSourceGenerationOptions(JsonSerializerDefaults.Web)]
internal sealed partial class PactMessageSerializationContext : JsonSerializerContext;
