using System.Text.Json;
using System.Text.Json.Serialization;

namespace BookWorm.Testing;

[JsonSerializable(typeof(PactMessageMetadata))]
[JsonSourceGenerationOptions(JsonSerializerDefaults.Web)]
internal sealed partial class PactSerializationContext : JsonSerializerContext;
