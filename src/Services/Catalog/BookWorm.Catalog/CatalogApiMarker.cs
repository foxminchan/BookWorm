using Vogen;

[assembly: VogenDefaults(
    openApiSchemaCustomizations: OpenApiSchemaCustomizations.GenerateOpenApiMappingExtensionMethod
)]

namespace BookWorm.Catalog;

[OpenApiMarker<AuthorId>]
[OpenApiMarker<BookId>]
[OpenApiMarker<CategoryId>]
[OpenApiMarker<PublisherId>]
public class CatalogApiMarker;
