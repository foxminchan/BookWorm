using BookWorm.Chassis.Endpoints;
using Microsoft.AspNetCore.OpenApi;
using Microsoft.Net.Http.Headers;
using Microsoft.OpenApi;

namespace BookWorm.ServiceDefaults.ApiSpecification.OpenApi.Transformers;

internal sealed class ETagOperationTransformer : IOpenApiOperationTransformer
{
    public Task TransformAsync(
        OpenApiOperation operation,
        OpenApiOperationTransformerContext context,
        CancellationToken cancellationToken
    )
    {
        var etagMetadata = context
            .Description.ActionDescriptor.EndpointMetadata.OfType<ETagMetadata>()
            .FirstOrDefault();

        if (etagMetadata is null)
        {
            return Task.CompletedTask;
        }

        if (etagMetadata.SupportsResponseETag)
        {
            operation.Parameters ??= [];
            AddHeaderParameterIfMissing(
                operation.Parameters,
                HeaderNames.IfNoneMatch,
                "Entity tag for conditional requests; returns 304 Not Modified when the resource has not changed"
            );

            if (operation.Responses is not null)
            {
                foreach (var (statusCode, response) in operation.Responses)
                {
                    if (
                        (
                            statusCode != $"{StatusCodes.Status304NotModified}"
                            && (
                                !statusCode.StartsWith('2')
                                || statusCode == $"{StatusCodes.Status204NoContent}"
                            )
                        ) || response is not OpenApiResponse concreteResponse
                    )
                    {
                        continue;
                    }

                    concreteResponse.Headers ??= new Dictionary<string, IOpenApiHeader>();
                    concreteResponse.Headers.TryAdd(
                        HeaderNames.ETag,
                        new OpenApiHeader
                        {
                            Description = "Entity tag representing the current resource version",
                            Schema = new OpenApiSchema { Type = JsonSchemaType.String },
                        }
                    );
                }
            }
        }

        if (!etagMetadata.SupportsIfMatch)
        {
            return Task.CompletedTask;
        }

        operation.Parameters ??= [];
        AddHeaderParameterIfMissing(
            operation.Parameters,
            HeaderNames.IfMatch,
            "Expected entity tag for optimistic concurrency control; returns 412 Precondition Failed when stale"
        );

        return Task.CompletedTask;
    }

    private static void AddHeaderParameterIfMissing(
        IList<IOpenApiParameter> parameters,
        string headerName,
        string description
    )
    {
        if (
            parameters.Any(p =>
                p.In == ParameterLocation.Header
                && string.Equals(p.Name, headerName, StringComparison.OrdinalIgnoreCase)
            )
        )
        {
            return;
        }

        parameters.Add(
            new OpenApiParameter
            {
                Name = headerName,
                In = ParameterLocation.Header,
                Required = false,
                Description = description,
                Schema = new OpenApiSchema { Type = JsonSchemaType.String },
            }
        );
    }
}
