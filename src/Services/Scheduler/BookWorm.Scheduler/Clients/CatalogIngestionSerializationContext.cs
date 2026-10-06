using System.Text.Json.Serialization;

namespace BookWorm.Scheduler.Clients;

[JsonSerializable(typeof(object))]
internal sealed partial class CatalogIngestionSerializationContext : JsonSerializerContext;
