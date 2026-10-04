using Vogen;

[assembly: VogenDefaults(
    openApiSchemaCustomizations: OpenApiSchemaCustomizations.GenerateOpenApiMappingExtensionMethod
)]

namespace BookWorm.Catalog;

[OpenApiMarker<AuthorId>]
[OpenApiMarker<BookId>]
[OpenApiMarker<CategoryId>]
[OpenApiMarker<PublisherId>]
[EfCoreConverter<AuthorId>]
[EfCoreConverter<BookId>]
[EfCoreConverter<CategoryId>]
[EfCoreConverter<PublisherId>]
public sealed partial class CatalogApiMarker;
