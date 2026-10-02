using System.Text.Json;
using System.Text.Json.Serialization;
using BookWorm.Contracts;

namespace BookWorm.Scheduler.ContractTests;

[JsonSerializable(typeof(CleanUpSentEmailIntegrationEvent))]
[JsonSerializable(typeof(ResendErrorEmailIntegrationEvent))]
[JsonSourceGenerationOptions(JsonSerializerDefaults.Web)]
internal sealed partial class PactMessageSerializationContext : JsonSerializerContext;
