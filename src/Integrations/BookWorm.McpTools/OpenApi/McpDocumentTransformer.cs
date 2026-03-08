using System.Net.Mime;
using System.Text.Json.Nodes;
using BookWorm.Chassis.Utilities;
using BookWorm.Constants.Core;
using Microsoft.AspNetCore.OpenApi;
using Microsoft.OpenApi;
using ModelContextProtocol.Protocol;

namespace BookWorm.McpTools.OpenApi;

internal sealed class McpDocumentTransformer(IHttpContextAccessor accessor)
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

        // Register JSON-RPC schemas as components
        var jsonRpcRequest = await context.GetOrCreateSchemaAsync(
            typeof(JsonRpcRequest),
            cancellationToken: cancellationToken
        );
        var jsonRpcNotification = await context.GetOrCreateSchemaAsync(
            typeof(JsonRpcNotification),
            cancellationToken: cancellationToken
        );
        var jsonRpcResponse = await context.GetOrCreateSchemaAsync(
            typeof(JsonRpcResponse),
            cancellationToken: cancellationToken
        );
        var jsonRpcError = await context.GetOrCreateSchemaAsync(
            typeof(JsonRpcError),
            cancellationToken: cancellationToken
        );

        document.AddComponent(nameof(JsonRpcRequest), jsonRpcRequest);
        document.AddComponent(nameof(JsonRpcNotification), jsonRpcNotification);
        document.AddComponent(nameof(JsonRpcResponse), jsonRpcResponse);
        document.AddComponent(nameof(JsonRpcError), jsonRpcError);

        // Build oneOf schema for request body per MCP Streamable HTTP spec:
        // "The body of the POST request MUST be a single JSON-RPC request, notification, or response."
        var jsonRpcMessage = new OpenApiSchema
        {
            OneOf =
            [
                new OpenApiSchemaReference(nameof(JsonRpcRequest), document),
                new OpenApiSchemaReference(nameof(JsonRpcNotification), document),
                new OpenApiSchemaReference(nameof(JsonRpcResponse), document),
            ],
        };
        document.AddComponent("JsonRpcMessage", jsonRpcMessage);

        var pathItem = new OpenApiPathItem();

        // POST /mcp - Send a JSON-RPC request, notification, or response
        pathItem.AddOperation(
            HttpMethod.Post,
            new()
            {
                Summary = "Invoke operation",
                Description =
                    "Send a JSON-RPC request, notification, or response to the MCP server.",
                Extensions = new Dictionary<string, IOpenApiExtension>
                {
                    ["x-ms-agentic-protocol"] = new JsonNodeExtension(
                        JsonValue.Create("mcp-streamable-1.0")
                    ),
                },
                OperationId = "InvokeMCP",
                Responses = new()
                {
                    [$"{StatusCodes.Status200OK}"] = new OpenApiResponse
                    {
                        Description = "Success - returned when the input is a JSON-RPC request",
                        Content = new Dictionary<string, OpenApiMediaType>
                        {
                            [MediaTypeNames.Application.Json] = new()
                            {
                                Schema = new OpenApiSchemaReference(
                                    nameof(JsonRpcResponse),
                                    document
                                ),
                            },
                            [MediaTypeNames.Text.EventStream] = new()
                            {
                                Schema = new OpenApiSchema
                                {
                                    Type = JsonSchemaType.String,
                                    Description =
                                        "Server-Sent Events stream containing JSON-RPC responses",
                                },
                            },
                        },
                    },
                    [$"{StatusCodes.Status202Accepted}"] = new OpenApiResponse
                    {
                        Description =
                            "Accepted - returned when the input is a JSON-RPC response or notification",
                    },
                    [$"{StatusCodes.Status400BadRequest}"] = new OpenApiResponse
                    {
                        Description =
                            "Bad Request - invalid JSON-RPC message, unsupported protocol version, or missing/invalid session ID",
                        Content = new Dictionary<string, OpenApiMediaType>
                        {
                            [MediaTypeNames.Application.Json] = new()
                            {
                                Schema = new OpenApiSchemaReference(nameof(JsonRpcError), document),
                            },
                        },
                    },
                    [$"{StatusCodes.Status403Forbidden}"] = new OpenApiResponse
                    {
                        Description =
                            "Forbidden - invalid Origin header, or the authenticated user does not match the user who initiated the session",
                        Content = new Dictionary<string, OpenApiMediaType>
                        {
                            [MediaTypeNames.Application.Json] = new()
                            {
                                Schema = new OpenApiSchemaReference(nameof(JsonRpcError), document),
                            },
                        },
                    },
                    [$"{StatusCodes.Status404NotFound}"] = new OpenApiResponse
                    {
                        Description = "Not Found - the specified session ID was not found",
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
                        Description =
                            "Not Acceptable - client must accept both application/json and text/event-stream",
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
                            Schema = new OpenApiSchemaReference("JsonRpcMessage", document),
                        },
                    },
                },
            }
        );

        // GET /mcp - Open SSE stream for server-initiated messages (stateful mode only)
        pathItem.AddOperation(
            HttpMethod.Get,
            new()
            {
                Summary = "Open SSE stream",
                Description =
                    "Open a Server-Sent Events stream to receive server-initiated JSON-RPC messages. Only available in stateful mode.",
                OperationId = "OpenMCPStream",
                Responses = new()
                {
                    [$"{StatusCodes.Status200OK}"] = new OpenApiResponse
                    {
                        Description = "SSE stream opened successfully",
                        Content = new Dictionary<string, OpenApiMediaType>
                        {
                            [MediaTypeNames.Text.EventStream] = new()
                            {
                                Schema = new OpenApiSchema
                                {
                                    Type = JsonSchemaType.String,
                                    Description =
                                        "Server-Sent Events stream containing JSON-RPC messages",
                                },
                            },
                        },
                    },
                    [$"{StatusCodes.Status400BadRequest}"] = new OpenApiResponse
                    {
                        Description =
                            "Bad Request - missing session ID, unsupported protocol version, or invalid Last-Event-ID",
                        Content = new Dictionary<string, OpenApiMediaType>
                        {
                            [MediaTypeNames.Application.Json] = new()
                            {
                                Schema = new OpenApiSchemaReference(nameof(JsonRpcError), document),
                            },
                        },
                    },
                    [$"{StatusCodes.Status404NotFound}"] = new OpenApiResponse
                    {
                        Description = "Not Found - the specified session ID was not found",
                        Content = new Dictionary<string, OpenApiMediaType>
                        {
                            [MediaTypeNames.Application.Json] = new()
                            {
                                Schema = new OpenApiSchemaReference(nameof(JsonRpcError), document),
                            },
                        },
                    },
                    [$"{StatusCodes.Status405MethodNotAllowed}"] = new OpenApiResponse
                    {
                        Description =
                            "Method Not Allowed - server does not offer an SSE stream at this endpoint",
                    },
                    [$"{StatusCodes.Status406NotAcceptable}"] = new OpenApiResponse
                    {
                        Description = "Not Acceptable - client must accept text/event-stream",
                        Content = new Dictionary<string, OpenApiMediaType>
                        {
                            [MediaTypeNames.Application.Json] = new()
                            {
                                Schema = new OpenApiSchemaReference(nameof(JsonRpcError), document),
                            },
                        },
                    },
                },
            }
        );

        // DELETE /mcp - Terminate a session (stateful mode only)
        pathItem.AddOperation(
            HttpMethod.Delete,
            new()
            {
                Summary = "Terminate session",
                Description =
                    "Terminate an active MCP session and clean up server-side resources. Only available in stateful mode.",
                OperationId = "TerminateMCPSession",
                Responses = new()
                {
                    [$"{StatusCodes.Status200OK}"] = new OpenApiResponse
                    {
                        Description = "Session terminated successfully",
                    },
                    [$"{StatusCodes.Status400BadRequest}"] = new OpenApiResponse
                    {
                        Description =
                            "Bad Request - missing session ID or unsupported protocol version",
                        Content = new Dictionary<string, OpenApiMediaType>
                        {
                            [MediaTypeNames.Application.Json] = new()
                            {
                                Schema = new OpenApiSchemaReference(nameof(JsonRpcError), document),
                            },
                        },
                    },
                    [$"{StatusCodes.Status404NotFound}"] = new OpenApiResponse
                    {
                        Description = "Not Found - the specified session ID was not found",
                        Content = new Dictionary<string, OpenApiMediaType>
                        {
                            [MediaTypeNames.Application.Json] = new()
                            {
                                Schema = new OpenApiSchemaReference(nameof(JsonRpcError), document),
                            },
                        },
                    },
                    [$"{StatusCodes.Status405MethodNotAllowed}"] = new OpenApiResponse
                    {
                        Description =
                            "Method Not Allowed - server does not allow clients to terminate sessions",
                    },
                },
            }
        );

        document.Paths.Add("/mcp", pathItem);
    }
}
