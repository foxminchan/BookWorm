using Vogen;

[assembly: VogenDefaults(
    openApiSchemaCustomizations: OpenApiSchemaCustomizations.GenerateOpenApiMappingExtensionMethod
)]

namespace BookWorm.Basket;

[OpenApiMarker<CustomerId>]
public sealed class BasketApiMarker;
