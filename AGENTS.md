# Project Contributor Guide

## Project Overview

BookWorm is a distributed microservices application built with .NET 8+, using Aspire for orchestration and a clean architecture approach. The project includes multiple services (Basket, Catalog, Chat, Finance, Notification, Ordering, Rating, Scheduler) with supporting building blocks and integration components.

## Setup Commands

- **Restore dependencies**: `dotnet restore`
- **Build solution**: `dotnet build`
- **Run with Aspire**: Navigate to `src/Aspire/BookWorm.AppHost` and run `dotnet run`
- **Run tests**: `dotnet test`
- **Architecture tests**: `dotnet test tests/BookWorm.ArchTests/`

## Development Environment

- Use C# 14 features and latest .NET version
- The solution uses Aspire for service orchestration and development
- Services are organized in the `src/Services/` directory
- Building blocks are in `src/BuildingBlocks/`
- All projects follow clean architecture principles

## Code Style Guidelines

- **Language**: Use C# 14 features exclusively
- **Formatting**: Follow `.editorconfig` rules
- **Namespaces**: Use file-scoped namespace declarations
- **Null Safety**: Declare variables non-nullable, use `is null`/`is not null`
- **Pattern Matching**: Use pattern matching and switch expressions where possible
- **Constructors**: Use primary constructors for immutable properties
- **Members**: Use expression-bodied members for methods and properties
- **Variables**: Use `var` when type is obvious
- **Naming**: Use `nameof` instead of string literals for member names
- **Structure**: Place private class declarations at bottom of files

## Architecture Patterns

- **Domain-Driven Design**: Each service has its own domain model
- **Clean Architecture**: Separate layers (Domain, Application, Infrastructure, API)
- **CQRS**: Command Query Responsibility Segregation where applicable
- **Event-Driven**: Services communicate via events
- **Microservices**: Each service is independently deployable

## Testing Instructions

- **Unit Tests**: Run `dotnet test` for all tests
- **Architecture Tests**: Use TUnit framework in `tests/BookWorm.ArchTests/`
- **Integration Tests**: Each service may have its own integration tests
- **Test Coverage**: Ensure new features have corresponding tests
- **Snapshot Testing**: Use `SnapshotTestBase` for snapshot tests when applicable

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

- **Clean solution**: `dotnet clean`
- **Format code**: Use IDE formatting or `dotnet format`
- **Package restore**: `dotnet restore --no-cache` (if needed)
- **Watch mode**: `dotnet watch run` (from service directory)

## Troubleshooting

- **Build Issues**: Check `Directory.Build.props` and `global.json` for version conflicts
- **Aspire Issues**: Ensure Docker is running if using containerized services
- **Test Failures**: Check test output and ensure proper test isolation
- **Architecture Violations**: Review architecture test failures for guidance

## File Modifications

- **Never change** `global.json` unless explicitly requested
- **Never change** `nuget.config` files unless explicitly requested
- **Always respect** the `.editorconfig` formatting rules
- **Maintain** the established project structure and patterns
