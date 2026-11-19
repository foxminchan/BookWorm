using System.Text.Json;
using System.Text.Json.Serialization;
using BookWorm.Contracts;

namespace BookWorm.Scheduler.ContractTests;

[JsonSerializable(typeof(ProcessVectorBatchIntegrationEvent))]
[JsonSourceGenerationOptions(JsonSerializerDefaults.Web)]
internal sealed partial class PactMessageSerializationContext : JsonSerializerContext;
