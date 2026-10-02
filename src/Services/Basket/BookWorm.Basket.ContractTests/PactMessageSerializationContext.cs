using System.Text.Json;
using System.Text.Json.Serialization;
using BookWorm.Contracts;

namespace BookWorm.Basket.ContractTests;

[JsonSerializable(typeof(PlaceOrderCommand))]
[JsonSerializable(typeof(BasketDeletedCompleteIntegrationEvent))]
[JsonSerializable(typeof(BasketDeletedFailedIntegrationEvent))]
[JsonSourceGenerationOptions(JsonSerializerDefaults.Web)]
internal sealed partial class PactMessageSerializationContext : JsonSerializerContext;
