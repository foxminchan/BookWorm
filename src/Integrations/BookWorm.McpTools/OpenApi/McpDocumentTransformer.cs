using System.Net.Mime;
using BookWorm.Constants.Core;
using Microsoft.AspNetCore.OpenApi;
using Microsoft.OpenApi.Any;
using Microsoft.OpenApi.Interfaces;
using Microsoft.OpenApi.Models;

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
            OperationType.Post,
            new()
            {
                Summary = "Get MCP Components",
                Extensions = new Dictionary<string, IOpenApiExtension>
                {
                    ["x-ms-agentic-protocol"] = new OpenApiString("mcp-streamable-1.0"),
                },
                OperationId = "InvokeMCP",
                Responses = new()
                {
                    [$"{StatusCodes.Status200OK}"] = new() { Description = "Success" },
                    [$"{StatusCodes.Status400BadRequest}"] = new() { Description = "Bad Request" },
                    [$"{StatusCodes.Status406NotAcceptable}"] = new()
                    {
                        Description = "Not Acceptable",
                    },
                },
                RequestBody = new()
                {
                    Content =
                    {
                        [MediaTypeNames.Application.Json] = new()
                        {
                            Schema = new()
                            {
                                Type = "object",
                                Properties =
                                {
                                    ["method"] = new() { Type = "string" },
                                    ["params"] = new()
                                    {
                                        Type = "object",
                                        Properties =
                                        {
                                            ["name"] = new() { Type = "string" },
                                            ["arguments"] = new() { Type = "object" },
                                        },
                                    },
                                    ["jsonrpc"] = new()
                                    {
                                        Type = "string",
                                        Enum = [new OpenApiString("1.0"), new OpenApiString("2.0")],
                                        Default = new OpenApiString("2.0"),
                                    },
                                    ["id"] = new() { Type = "integer" },
                                },
                                Required = new HashSet<string>
                                {
                                    "method",
                                    "params",
                                    "jsonrpc",
                                    "id",
                                },
                            },
                        },
                    },
                },
            }
        );

        document.Paths ??= [];
        document.Paths.Add("/mcp", pathItem);

        return Task.CompletedTask;
    }
}
