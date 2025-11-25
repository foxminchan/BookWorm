using System.Net.Mime;
using BookWorm.Chassis.Utilities;
using BookWorm.Constants.Core;
using Microsoft.AspNetCore.OpenApi;
using Microsoft.OpenApi;
using ModelContextProtocol.Protocol;

namespace BookWorm.McpTools.OpenApi;

public sealed class McpDocumentTransformer(IHttpContextAccessor accessor)
    : IOpenApiDocumentTransformer
{
    public async Task TransformAsync(
        OpenApiDocument document,
        OpenApiDocumentTransformerContext context,
        CancellationToken cancellationToken
    )
    {
        document.Servers =
        [
            new()
            {
                Url = accessor.HttpContext?.Request is { } request
                    ? HttpUtilities
                        .AsUrlBuilder()
                        .WithScheme(request.Scheme)
                        .WithHost(request.Host)
                        .Build()
                    : HttpUtilities
                        .AsUrlBuilder()
                        .WithScheme(Http.Schemes.Https)
                        .WithHost(Network.Localhost)
                        .WithPort(8080)
                        .Build(),
            },
        ];

        var jsonRpcResponse = context.GetOrCreateSchemaAsync(
            typeof(JsonRpcResponse),
            cancellationToken: cancellationToken
        );

        var jsonRpcRequest = context.GetOrCreateSchemaAsync(
            typeof(JsonRpcRequest),
            cancellationToken: cancellationToken
        );

        var jsonRpcError = context.GetOrCreateSchemaAsync(
            typeof(JsonRpcError),
            cancellationToken: cancellationToken
        );

        await Task.WhenAll(jsonRpcResponse, jsonRpcRequest, jsonRpcError);

        document.AddComponent(nameof(JsonRpcResponse), await jsonRpcResponse);

        document.AddComponent(nameof(JsonRpcRequest), await jsonRpcRequest);

        document.AddComponent(nameof(JsonRpcError), await jsonRpcError);

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
                                Schema = new OpenApiSchemaReference(
                                    nameof(JsonRpcResponse),
                                    document
                                ),
                            },
                        },
                    },
                    [$"{StatusCodes.Status400BadRequest}"] = new OpenApiResponse
                    {
                        Description = "Bad Request",
                        Content = new Dictionary<string, OpenApiMediaType>
                        {
                            [MediaTypeNames.Application.Json] = new()
                            {
                                Schema = new OpenApiSchemaReference(nameof(JsonRpcError), document),
                            },
                        },
                    },
                    [$"{StatusCodes.Status406NotAcceptable}"] = new OpenApiResponse
                    {
                        Description = "Not Acceptable",
                        Content = new Dictionary<string, OpenApiMediaType>
                        {
                            [MediaTypeNames.Application.Json] = new()
                            {
                                Schema = new OpenApiSchemaReference(nameof(JsonRpcError), document),
                            },
                        },
                    },
                },
                RequestBody = new OpenApiRequestBody
                {
                    Required = true,
                    Content = new Dictionary<string, OpenApiMediaType>
                    {
                        [MediaTypeNames.Application.Json] = new()
                        {
                            Schema = new OpenApiSchemaReference(nameof(JsonRpcRequest), document),
                        },
                    },
                },
            }
        );

        document.Paths.Add("/mcp", pathItem);
    }
}
