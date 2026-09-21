using BookWorm.Rating.Features;
using Vogen;

[assembly: VogenDefaults(
    openApiSchemaCustomizations: OpenApiSchemaCustomizations.GenerateOpenApiMappingExtensionMethod
)]

namespace BookWorm.Rating;

[OpenApiMarker<FeedbackId>]
[OpenApiMarker<BookId>]
public class RatingApiMarker;
