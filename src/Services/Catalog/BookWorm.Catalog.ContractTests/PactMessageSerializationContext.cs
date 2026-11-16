using System.Text.Json;
using System.Text.Json.Serialization;
using BookWorm.Contracts;

namespace BookWorm.Catalog.ContractTests;

[JsonSerializable(typeof(FeedbackCreatedIntegrationEvent))]
[JsonSerializable(typeof(FeedbackDeletedIntegrationEvent))]
[JsonSerializable(typeof(BookUpdatedRatingFailedIntegrationEvent))]
[JsonSerializable(typeof(ProcessVectorBatchIntegrationEvent))]
[JsonSourceGenerationOptions(JsonSerializerDefaults.Web)]
internal sealed partial class PactMessageSerializationContext : JsonSerializerContext;
