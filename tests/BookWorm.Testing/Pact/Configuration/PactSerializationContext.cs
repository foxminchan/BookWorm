using System.Text.Json;
using System.Text.Json.Serialization;
using BookWorm.Testing.Pact.Messaging;

namespace BookWorm.Testing.Pact.Configuration;

[JsonSerializable(typeof(PactMessageMetadata))]
[JsonSourceGenerationOptions(JsonSerializerDefaults.Web)]
internal sealed partial class PactSerializationContext : JsonSerializerContext;
