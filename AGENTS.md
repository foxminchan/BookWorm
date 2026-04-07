# Agent Instructions

## General recommendations for working with Aspire

1. Before making any changes always run the apphost using `aspire run` and inspect the state of resources to make sure you are building from a known state.
1. Changes to the _AppHost.cs_ file will require a restart of the application to take effect.
1. Make changes incrementally and run the aspire application using the `aspire run` command to validate changes.
1. Use the Aspire MCP tools to check the status of resources and debug issues.

## Getting Started

The project uses a `.justfile` for common tasks. Run these commands:

- **Restore dependencies**: `just restore` (or `dotnet restore && dotnet tool restore`)
- **Build solution**: `just build` (or `dotnet build`)
- **Run with Aspire**: `just run` (handles setup + runs Aspire)
- **Run tests**: `just test` (or `dotnet test`)
- **Format code**: `just format` (formats C#, frontend, eventcatalog, docusaurus, k6, keycloakify)
- **Clean build**: `just clean`
- **Post-clone setup**: `just prepare` (restore + git hooks)

If there is already an instance of the application running it will prompt to stop the existing instance. You only need to restart the application if code in `apphost.cs` is changed, but if you experience problems it can be useful to reset everything to the starting state.

## Local Development Flow

1. **Prerequisites**: .NET SDK (per `global.json`), Node.js ≥25, Docker for AppHost resources.
2. **Restore & build**: `just restore` then `just build`, or `dotnet restore && dotnet build`.
3. **Run the system**: Launch Aspire AppHost — inspect the dashboard URL from console output.
4. **Frontend**: From `src/Clients/`: `pnpm i`, then `pnpm run dev`.
5. **Secrets**: Use User Secrets or environment variables for API keys. Never commit secrets.
6. **Tests**: `dotnet test` from the solution root or specific test projects.

## Project Structure

- `src/Aspire/`: Aspire host and service defaults
- `src/BuildingBlocks/`: Shared libraries (Chassis, Constants, SharedKernel)
- `src/Clients/`: Frontend applications (Next.js 16 Turbo monorepo)
  - `apps/storefront/`: Customer-facing storefront
  - `apps/backoffice/`: Admin dashboard
  - `packages/`: Shared packages (api-client, api-hooks, ui, types, validations, utils, mocks, eslint-config, typescript-config)
- `src/Services/`: Individual microservices (Catalog, Ordering, Basket, Rating, Chat, Finance, Notification, Scheduler, McpTools)
- `src/Integrations/`: Integration components (HealthChecksUI, Presidio)
- `tests/`: Cross-cutting test projects (architecture tests, AI evaluation)
- `docs/`: Documentation (EventCatalog, Docusaurus)
- `specs/`: Feature specifications (e.g., migration plans)

## Key Code Patterns

### Adding a New Feature

Features follow Vertical Slice Architecture. To add a feature to a service:

1. Create `Features/{FeatureName}/` folder in the service project
2. Add a command/query as `sealed record` implementing `ICommand<T>` or `IQuery<T>`
3. Add an `internal sealed` handler with primary constructor for DI; returns `ValueTask<T>`
4. Add an endpoint class implementing `IEndpoint` (1–4 type params) with `MapEndpoint()` and `HandleAsync()`
5. The endpoint is auto-discovered — no manual route registration needed

### Adding a New Endpoint

```csharp
public sealed class MyEndpoint : IEndpoint<Ok<MyResult>, MyRequest, ISender>
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapGet("/my-route", async (MyRequest request, ISender sender)
                => await HandleAsync(request, sender))
            .ProducesGet<MyResult>()
            .MapToApiVersion(ApiVersions.V1);
    }

    public async Task<Ok<MyResult>> HandleAsync(
        MyRequest request, ISender sender,
        CancellationToken cancellationToken = default)
    {
        var result = await sender.Send(new MyQuery(request), cancellationToken);
        return TypedResults.Ok(result);
    }
}
```

### Infrastructure

- **Database**: Each service has its own PostgreSQL database, configured with snake_case naming and UUID v7 keys
- **Events**: MassTransit with Kafka; use Outbox/Inbox for transactional consistency
- **Caching**: HybridCache (distributed + local tiers)
- **Vector DB**: Qdrant for embedding-based search (Catalog, Rating)
- **PII**: Presidio Analyzer + Anonymizer for PII detection/redaction
- **Auth**: Keycloak with token introspection; use `.RequireAuthorization()` on endpoints
- **Gateway**: YARP reverse proxy
- **API Versioning**: Asp.Versioning with `ApiVersions.V1`

## Common Pitfalls

- **Handlers**: Must be `internal sealed` with primary constructors; return `ValueTask<T>` (not `Task<T>`).
- **CQRS library**: Use `Mediator.SourceGenerator` (source-gen based), NOT MediatR. The interface names look similar but the packages differ.
- **snake_case DB**: PostgreSQL columns/tables are snake_case via `UseSnakeCaseNamingConvention()`. Don't use PascalCase in raw SQL.
- **Sealed classes**: All endpoints, handlers, tests, and DbContexts should be `sealed`.
- **Warnings = Errors**: `TreatWarningsAsErrors=true` — the build will fail on any warning.
- **Package versions**: Centralized in `Directory.Packages.props` — don't add version numbers in individual `.csproj` files.
- **Test projects**: Follow `BookWorm.{Service}.{UnitTests|ContractTests|IntegrationTests}` naming. Tests are auto-detected by project name suffix.

## Documentation

- **Architecture**: See `docs/docusaurus/` for detailed documentation
- **Events**: Event schemas and documentation in `docs/eventcatalog/`
- **API**: OpenAPI specifications in `docs/eventcatalog/openapi-files/`

## Official documentation

IMPORTANT! Always prefer official documentation when available. The following sites contain the official documentation for Aspire and related components

1. https://aspire.dev
2. https://learn.microsoft.com/dotnet/aspire
3. https://nuget.org (for specific integration package details)
