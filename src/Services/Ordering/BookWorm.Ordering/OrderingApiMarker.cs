using Vogen;

[assembly: VogenDefaults(
    openApiSchemaCustomizations: OpenApiSchemaCustomizations.GenerateOpenApiMappingExtensionMethod
)]

namespace BookWorm.Ordering;

[OpenApiMarker<BuyerId>]
[OpenApiMarker<OrderId>]
public class OrderingApiMarker;
