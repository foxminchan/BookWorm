# Project Contributor Guide

## Project Overview

BookWorm is a distributed microservices application built with .NET 10, using Aspire 13+ for orchestration and a clean architecture approach. The project showcases:

- **Microservices Architecture**: Multiple services (Basket, Catalog, Chat, Finance, Notification, Ordering, Rating, Scheduler)
- **AI Integration**: Multi-agent orchestration, Model Context Protocol (MCP), Agent-to-Agent (A2A) communication
- **DDD & VSA**: Domain-Driven Design with Vertical Slice Architecture
- **Event-Driven**: Saga patterns, event sourcing, outbox/inbox patterns
- **Supporting components**: Building blocks and integration components

## Prerequisites

Before contributing, ensure you have:

- [.NET 10.0 SDK](https://dotnet.microsoft.com/download/dotnet/10.0)
- [Docker Desktop](https://www.docker.com/get-started) (must be running)
- [Aspire CLI](https://learn.microsoft.com/en-us/dotnet/aspire/cli/install)
- [Node.js](https://nodejs.org/en/download/)
- [Bun](https://bun.sh/)
- [Just](https://github.com/casey/just) (task runner)
- **OpenAI API key** (required for AI features) - [Get one here](https://platform.openai.com/api-keys)
- Optional: [Gitleaks](https://gitleaks.io/), [Azure CLI](https://learn.microsoft.com/cli/azure/install-azure-cli)

## Setup Commands

The project uses a `.justfile` for common tasks. Run these commands:

- **Restore dependencies**: `just restore` (or `dotnet restore`)
- **Build solution**: `just build` (or `dotnet build`)
- **Run with Aspire**: `just run` (handles setup + runs Aspire)
- **Run tests**: `just test` (or `dotnet test`)
- **Format code**: `just format`
- **Clean build**: `just clean`
- **Trust dev certificate**: `just trust`
- **Update packages**: `just update`

> [!TIP]
> Run `just` without arguments to see the default action (runs the application).

## Development Environment

- **Framework**: .NET 10 with C# preview features
- **Orchestration**: Aspire 13+ for service orchestration and local development
- **Project Structure**:
  - `src/Services/` - Individual microservices
  - `src/BuildingBlocks/` - Shared libraries (Chassis, Constants, SharedKernel)
  - `src/Integrations/` - Integration components (HealthChecks, MCP Tools)
  - `src/Aspire/` - Aspire host and service defaults
- **Architecture**: Clean architecture with DDD and Vertical Slice Architecture principles
- **Authentication**: Keycloak for AuthN/AuthZ (Authorization Code Flow with PKCE, Token Exchange)

## Code Style Guidelines

- **Language**: Use C# preview features (configured in `Directory.Build.props`)
- **Target Framework**: .NET 10 (`net10.0`)
- **Formatting**: Follow `.editorconfig` rules; use `just format` to apply CSharpier formatting
- **Namespaces**: Use file-scoped namespace declarations
- **Null Safety**: Enable nullable reference types; declare variables non-nullable, use `is null`/`is not null`
- **Pattern Matching**: Use pattern matching and switch expressions where possible
- **Constructors**: Use primary constructors for immutable properties
- **Members**: Use expression-bodied members for methods and properties
- **Variables**: Use `var` when type is obvious from the right side
- **Naming**: Use `nameof()` instead of string literals for member names
- **Structure**: Place private nested classes at the bottom of files
- **Warnings**: Treat warnings as errors (`TreatWarningsAsErrors` is enabled)

## Architecture Patterns

- **Domain-Driven Design**: Each service has its own domain model
- **Clean Architecture**: Separate layers (Domain, Application, Infrastructure, API)
- **CQRS**: Command Query Responsibility Segregation where applicable
- **Event-Driven**: Services communicate via events
- **Microservices**: Each service is independently deployable

## Testing Instructions

- **Unit Tests**: Run `just test` or `dotnet test` for all tests
- **Test Framework**: Uses **TUnit** (not xUnit/NUnit/MSTest)
- **Architecture Tests**: Located in `tests/BookWorm.ArchTests/` using ArchUnitNET with TUnit
- **Snapshot Tests**: Use `Verify.TUnit` for snapshot testing
- **Contract Tests**: Service contract tests in `*ContractTests` projects
- **Integration Tests**: Each service may have integration tests using `Aspire.Hosting.Testing`
- **Coverage**: Run tests with coverage using Microsoft.Testing.Extensions.CodeCoverage
- **Test Naming**: Follow TUnit conventions for test methods and classes
- **Test Coverage**: Ensure new features have corresponding tests
- **Assertion Library**: Use Shouldly for fluent assertions

### Running Specific Tests

```powershell
# Run all tests
just test

# Run architecture tests only
dotnet test tests/BookWorm.ArchTests/

# Run tests for a specific service
dotnet test src/Services/Finance/BookWorm.Finance.UnitTests/

# Run with coverage
dotnet test --collect:"Code Coverage"
```

## Build and Deployment

- **Solution File**: Use `BookWorm.slnx` for the main solution
- **Dependencies**: Managed via `Directory.Packages.props`
- **Versioning**: Controlled by `Versions.props`
- **Docker**: Services can be containerized (check individual service directories)
- **Azure**: Uses `azure.yaml` for Azure deployment configuration

## Project Structure

- `src/Aspire/`: Aspire host and service defaults
- `src/BuildingBlocks/`: Shared libraries (Chassis, Constants, SharedKernel)
- `src/Services/`: Individual microservices
- `src/Integrations/`: Integration components (HealthChecks, MCP Tools)
- `tests/`: Test projects including architecture tests
- `docs/`: Documentation including EventCatalog and Docusaurus

## Security Considerations

- Services should validate all inputs
- Use proper authentication and authorization
- Follow OWASP guidelines for web applications
- Sensitive configuration should use secure storage

## Development Workflow

1. **Feature Development**: Create feature branches from `main`
2. **Code Changes**: Follow the architectural patterns and code style
3. **Testing**: Write unit and integration tests for new features
4. **Architecture Validation**: Run architecture tests to ensure compliance
5. **Documentation**: Update relevant documentation if needed

## Service-Specific Notes

Each service in `src/Services/` follows the same pattern:

- API layer for HTTP endpoints
- Application layer for business logic
- Domain layer for core business rules
- Infrastructure layer for external dependencies

## Documentation

- **Architecture**: See `docs/docusaurus/` for detailed documentation
- **Events**: Event schemas and documentation in `docs/eventcatalog/`
- **API**: OpenAPI specifications in `docs/eventcatalog/openapi-files/`

## Common Commands

- **Clean solution**: `just clean` or `dotnet clean`
- **Format code**: `just format` (uses CSharpier via dnx)
- **Package restore**: `just restore` or `dotnet restore --no-cache`
- **Watch mode**: `dotnet watch run` (from service directory)
- **Trust dev certs**: `just trust` or `dotnet dev-certs https --trust`
- **Update bun packages**: `just update` (updates EventCatalog, Docusaurus, K6, Keycloakify)
- **Check OpenAI key**: `just check-openai` (validates OpenAI API key setup)

## Troubleshooting

- **Build Issues**: Check `Directory.Build.props` and `global.json` for version conflicts
- **Aspire Issues**: Ensure Docker Desktop is running before launching Aspire
- **Test Failures**: Check test output and ensure proper test isolation
- **Architecture Violations**: Review architecture test failures for guidance on fixing structural issues
- **OpenAI Errors**: Verify your OpenAI API key is configured in User Secrets or environment variables
- **Authentication Issues**: Check Keycloak container is running and properly configured
- **Missing Dependencies**: Run `just restore` to restore all NuGet packages and tools

## File Modifications

- **Never change** `global.json` unless explicitly requested
- **Never change** `nuget.config` files unless explicitly requested
- **Always respect** the `.editorconfig` formatting rules
- **Maintain** the established project structure and patterns
