using System.Text.Json;
using System.Text.Json.Serialization;
using BookWorm.Contracts;

namespace BookWorm.Notification.ContractTests;

[JsonSerializable(typeof(PlaceOrderCommand))]
[JsonSerializable(typeof(CompleteOrderCommand))]
[JsonSerializable(typeof(CancelOrderCommand))]
[JsonSerializable(typeof(CleanUpSentEmailIntegrationEvent))]
[JsonSerializable(typeof(ResendErrorEmailIntegrationEvent))]
[JsonSourceGenerationOptions(JsonSerializerDefaults.Web)]
internal sealed partial class PactMessageSerializationContext : JsonSerializerContext;
