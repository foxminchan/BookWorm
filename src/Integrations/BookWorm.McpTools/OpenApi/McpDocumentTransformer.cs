using System.Net.Mime;
using BookWorm.Constants.Core;
using Microsoft.AspNetCore.OpenApi;
using Microsoft.OpenApi;
using ModelContextProtocol.Protocol;

namespace BookWorm.McpTools.OpenApi;

public sealed class McpDocumentTransformer(IHttpContextAccessor accessor)
    : IOpenApiDocumentTransformer
{
    public Task TransformAsync(
        OpenApiDocument document,
        OpenApiDocumentTransformerContext context,
        CancellationToken cancellationToken
    )
    {
        document.Servers =
        [
            new()
            {
                Url = accessor.HttpContext is not null
                    ? $"{accessor.HttpContext.Request.Scheme}://{accessor.HttpContext.Request.Host}/"
                    : $"{Protocols.Https}://{Restful.Host.Localhost}:8080/",
            },
        ];

        var pathItem = new OpenApiPathItem();

        pathItem.AddOperation(
            HttpMethod.Post,
            new()
            {
                Summary = "Get MCP Components",
                Extensions = new Dictionary<string, IOpenApiExtension>
                {
                    ["x-ms-agentic-protocol"] = new JsonNodeExtension("mcp-streamable-1.0"),
                },
                OperationId = "InvokeMCP",
                Responses = new()
                {
                    [$"{StatusCodes.Status200OK}"] = new OpenApiResponse
                    {
                        Description = "Success",
                        Content = new Dictionary<string, OpenApiMediaType>
                        {
                            [MediaTypeNames.Application.Json] = new()
                            {
                                Schema = new OpenApiSchemaReference(nameof(JsonRpcResponse)),
                            },
                        },
                    },
                    [$"{StatusCodes.Status400BadRequest}"] = new OpenApiResponse
                    {
                        Description = "Bad Request",
                    },
                    [$"{StatusCodes.Status406NotAcceptable}"] = new OpenApiResponse
                    {
                        Description = "Not Acceptable",
                    },
                },
                RequestBody = new OpenApiRequestBody
                {
                    Required = true,
                    Content = new Dictionary<string, OpenApiMediaType>
                    {
                        [MediaTypeNames.Application.Json] = new()
                        {
                            Schema = new OpenApiSchemaReference(nameof(JsonRpcRequest)),
                        },
                    },
                },
            }
        );

        document.Paths.Add("/mcp", pathItem);

        return Task.CompletedTask;
    }
}
