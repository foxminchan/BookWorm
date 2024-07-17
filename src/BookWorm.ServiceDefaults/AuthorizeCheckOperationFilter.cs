using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace BookWorm.ServiceDefaults;

public sealed class AuthorizeCheckOperationFilter(string[] scopes) : IOperationFilter
{
    public void Apply(OpenApiOperation operation, OperationFilterContext context)
    {
        var metadata = context.ApiDescription.ActionDescriptor.EndpointMetadata;

        if (!metadata.OfType<IAuthorizeData>().Any()) return;

        operation.Responses.TryAdd(StatusCodes.Status401Unauthorized.ToString(),
            new() { Description = "Unauthorized" });
        operation.Responses.TryAdd(StatusCodes.Status403Forbidden.ToString(), new() { Description = "Forbidden" });

        OpenApiSecurityScheme oAuthScheme = new()
        {
            Reference = new() { Type = ReferenceType.SecurityScheme, Id = "oauth2" }
        };

        operation.Security =
        [
            new() { [oAuthScheme] = scopes }
        ];
    }
}
