using System.Net.Mime;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace BookWorm.Scalar;

public static class ScalarEndpointRouteBuilderExtensions
{
    public static void MapScalarApiReference(this IEndpointRouteBuilder endpoints)
    {
        endpoints
            .MapGet(
                "/scalar/{resourceName}/{documentName}",
                (string resourceName, string documentName) =>
                {
                    var openApiUrl = $"/openapi/{resourceName}/{documentName}.json";
                    return Results.Content(
                        $"""
                        <!doctype html>
                        <html>
                        <head>
                            <title>{resourceName} - {documentName}</title>
                            <meta charset="utf-8" />
                            <meta name="viewport" content="width=device-width, initial-scale=1" />
                            <link rel="shortcut icon" href="https://scalar.com/favicon.png">
                        </head>
                        <body>
                            <script id="api-reference" data-url="{openApiUrl}"></script>
                            <script src="https://cdn.jsdelivr.net/npm/@scalar/api-reference"></script>
                        </body>
                        </html>
                        """,
                        MediaTypeNames.Text.Html
                    );
                }
            )
            .ExcludeFromDescription();
    }
}
