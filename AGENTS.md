# Agent Instructions

## General recommendations for working with Aspire

1. Before making any changes always run the apphost using `aspire run` and inspect the state of resources to make sure you are building from a known state.
1. Changes to the _AppHost.cs_ file will require a restart of the application to take effect.
1. Make changes incrementally and run the aspire application using the `aspire run` command to validate changes.
1. Use the Aspire MCP tools to check the status of resources and debug issues.

## Getting Started

The project uses a `.justfile` for common tasks. Run these commands:

- **Restore dependencies**: `just restore` (or `dotnet restore`)
- **Build solution**: `just build` (or `dotnet build`)
- **Run with Aspire**: `just run` (handles setup + runs Aspire)
- **Run tests**: `just test` (or `dotnet test`)
- **Format code**: `just format`
- **Clean build**: `just clean`
- **Update packages**: `just update`

If there is already an instance of the application running it will prompt to stop the existing instance. You only need to restart the application if code in `apphost.cs` is changed, but if you experience problems it can be useful to reset everything to the starting state.

## Local Development Flow

1. **Prerequisites**: .NET SDK (per `global.json`), Node.js, Docker for AppHost resources.
2. **Restore & build**: `just restore` then `just build`, or `dotnet restore && dotnet build`.
3. **Run the system**: Launch Aspire AppHost — inspect the dashboard URL from console output.
4. **Frontend**: From `src/Clients/`: `pnpm i`, then `pnpm run dev`.
5. **Secrets**: Use User Secrets or environment variables for API keys. Never commit secrets.
6. **Tests**: `dotnet test` from the solution root or specific test projects.

## Project Structure

- `src/Aspire/`: Aspire host and service defaults
- `src/BuildingBlocks/`: Shared libraries (Chassis, Constants, SharedKernel)
- `src/Clients/`: Frontend applications (Next.js 16 monorepo)
- `src/Services/`: Individual microservices
- `src/Integrations/`: Integration components (HealthChecks, MCP Tools)
- `tests/`: Test projects including architecture tests
- `docs/`: Documentation including EventCatalog and Docusaurus

## Documentation

- **Architecture**: See `docs/docusaurus/` for detailed documentation
- **Events**: Event schemas and documentation in `docs/eventcatalog/`
- **API**: OpenAPI specifications in `docs/eventcatalog/openapi-files/`

## Official documentation

IMPORTANT! Always prefer official documentation when available. The following sites contain the official documentation for Aspire and related components

1. https://aspire.dev
2. https://learn.microsoft.com/dotnet/aspire
3. https://nuget.org (for specific integration package details)
