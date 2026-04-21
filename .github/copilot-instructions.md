# BookWorm — Copilot Instructions

## Project Identity

BookWorm is a .NET 10 microservices bookstore using Aspire orchestration, DDD with Clean Architecture, and event-driven patterns (MassTransit/Kafka).

## Tech Stack

- **Backend**: C# 14 (`LangVersion=preview`), .NET 10, ASP.NET Core Minimal APIs, EF Core 10 + PostgreSQL (snake_case)
- **Frontend**: TypeScript 6.0, Next.js 16.2, React 19, pnpm 10 + Turbo 2 monorepo (Node >= 25)
- **CQRS**: `Mediator.SourceGenerator` (source generator-based, NOT MediatR) — uses `ICommand<T>`/`IQuery<T>` and `ICommandHandler`/`IQueryHandler`
- **Testing**: TUnit, Moq, Bogus, Shouldly, Verify.TUnit
- **Messaging**: MassTransit with Kafka (outbox/inbox patterns)
- **AI**: Microsoft Agents AI Framework (incl. A2A), Semantic Kernel, MCP server, CopilotKit (storefront)
- **Auth**: Keycloak with token introspection + Keycloakify theme
- **Gateway**: YARP reverse proxy (Aspire-hosted, routes all service traffic)

## Services

| Service          | Purpose                                               |
| ---------------- | ----------------------------------------------------- |
| **Catalog**      | Book catalog, search, embedding generation, inventory |
| **Ordering**     | Order processing, saga orchestration                  |
| **Basket**       | Shopping cart (Redis-backed)                          |
| **Rating**       | Feedback/reviews, LLM-based summarization             |
| **Chat**         | Conversational AI, multi-agent orchestration          |
| **Finance**      | Payment processing, billing                           |
| **Notification** | Email via MJML templates (SendGrid/MailKit)           |
| **Scheduler**    | Job scheduling (Quartz)                               |
| **McpTools**     | MCP server exposing catalog/rating tools to LLMs      |

## Coding Standards

- Use latest C# 14 features; never modify `global.json` or `NuGet.config` unless asked
- Use `IEndpoint<TResult, TRequest>` pattern from BookWorm.Chassis for Minimal API endpoints
- Follow DDD aggregate boundaries; business logic belongs in the domain layer
- Use `async`/`await` end-to-end with `CancellationToken` propagation
- Prefer `var` when type is obvious; use pattern matching and switch expressions
- Apply file-scoped namespaces and primary constructors
- Never commit secrets or API keys; use User Secrets for local dev
- Validate inputs at service boundaries; scrub PII in logs
- All public APIs require XML doc comments
- All warnings are errors (`TreatWarningsAsErrors=true`)
- Centralized package versioning via `Directory.Packages.props`

## Key Patterns

### Endpoint Pattern (Minimal API)

Implement `IEndpoint<TResult, TRequest>` from BookWorm.Chassis. Endpoints are sealed classes with `MapEndpoint()` for route registration and `HandleAsync()` that delegates to CQRS via `ISender`. Chain `.ProducesGet<T>()`, `.MapToApiVersion()`, `.RequireAuthorization()`.

### CQRS (Vertical Slice)

Features live in `Features/{FeatureName}/` per service. Each feature folder contains:

- Command/Query record implementing `ICommand<T>` or `IQuery<T>`
- Handler class implementing `ICommandHandler` or `IQueryHandler`
- Endpoint class implementing `IEndpoint`

### EF Core

- UUID v7 for IDs (`UniqueIdentifierHelper.NewUuidV7`)
- Value objects via `OwnsOne()`; soft deletes via `HasQueryFilter(x => !x.IsDeleted)`
- `IEntityTypeConfiguration<T>` in `Infrastructure/EntityConfigurations/`
- MassTransit Inbox/Outbox entities registered in `OnModelCreating`
- snake_case naming convention via `UseSnakeCaseNamingConvention()`

### Data Access

`IRepository<T>` + `IUnitOfWork` pattern; repositories encapsulate aggregate persistence

### Testing

- Name: `GivenCondition_WhenAction_ThenExpectedResult()`
- Sealed test classes; Bogus Fakers for test data; Arrange-Act-Assert
- Projects: `{Service}.UnitTests`, `{Service}.ContractTests`, `{Service}.IntegrationTests`

## Key Locations

- AppHost: `src/Aspire/BookWorm.AppHost/AppHost.cs`
- Services: `src/Services/{Name}/BookWorm.{Name}/`
- Frontend: `src/Clients/` (Turbo monorepo with `apps/` and `packages/`)
- Shared: `src/BuildingBlocks/` (Chassis, Constants, SharedKernel)
- Integrations: `src/Integrations/` (Presidio PII detection/redaction)
- Gateway: YARP reverse proxy defined in `src/Aspire/BookWorm.AppHost/Extensions/Network/ProxyExtensions.cs`
- Tests: `tests/` (architecture tests, AI evaluation), `src/Services/{Name}/BookWorm.{Name}.UnitTests/`
- Specs: `specs/` (feature specifications)
- Docs: `docs/docusaurus/` (architecture), `docs/eventcatalog/` (event schemas)

<!-- SPECKIT START -->

For additional context about technologies to be used, project structure,
shell commands, and other important information, read the current plan

<!-- SPECKIT END -->
