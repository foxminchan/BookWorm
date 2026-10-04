using Vogen;

[assembly: VogenDefaults(
    openApiSchemaCustomizations: OpenApiSchemaCustomizations.GenerateOpenApiMappingExtensionMethod
)]

namespace BookWorm.Ordering;

[OpenApiMarker<BuyerId>]
[OpenApiMarker<OrderId>]
[EfCoreConverter<BuyerId>]
[EfCoreConverter<OrderId>]
public sealed partial class OrderingApiMarker;
