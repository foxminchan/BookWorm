using System.Text.Json;
using System.Text.Json.Serialization;
using BookWorm.Contracts;

namespace BookWorm.Rating.ContractTests;

[JsonSerializable(typeof(FeedbackCreatedIntegrationEvent))]
[JsonSerializable(typeof(FeedbackDeletedIntegrationEvent))]
[JsonSerializable(typeof(BookUpdatedRatingFailedIntegrationEvent))]
[JsonSourceGenerationOptions(JsonSerializerDefaults.Web)]
internal sealed partial class PactMessageSerializationContext : JsonSerializerContext;
