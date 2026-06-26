# BookWorm — Copilot Instructions

## Project Identity

BookWorm is a .NET 10 microservices bookstore using Aspire orchestration, DDD with Clean Architecture, and event-driven patterns (WolverineFx/Kafka).

## Tech Stack

- **Backend**: C# 14 (`LangVersion=preview`), .NET 10, ASP.NET Core Minimal APIs, EF Core 10 + PostgreSQL (snake_case)
- **Frontend**: TypeScript 6.0, Next.js 16.2, React 19, Bun 1.3 + Turbo 2 monorepo (Node >= 24)
- **CQRS**: `Mediator.SourceGenerator` (source generator-based, NOT MediatR) — uses `ICommand<T>`/`IQuery<T>` and `ICommandHandler`/`IQueryHandler`
- **Testing**: TUnit, Moq, Bogus, Shouldly, Verify.TUnit
- **Messaging**: WolverineFx with Kafka (outbox/inbox patterns)
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

## Commands

Tasks are defined in [mise.toml](../mise.toml). Prefer `mise run` over raw `dotnet`/`bun` so dependencies resolve correctly:

- `mise run restore` — restore NuGet packages + .NET tools
- `mise run build` — build the solution (`BookWorm.slnx`)
- `mise run test` — run all tests
- `mise run run` — start the Aspire AppHost (use `aspire start` directly when iterating)
- `mise run format` — format C# (CSharpier), frontend, EventCatalog, Docusaurus, k6, Keycloakify
- `mise run prepare` — post-clone setup (restore + git hooks)

Frontend dev: from `src/Clients/` run `bun i && bun run dev`.

## Common Pitfalls

- **Mediator ≠ MediatR**: this repo uses `Mediator.SourceGenerator` (source-generator-based). Same-looking interfaces, different package — do not add MediatR.
- **Warnings = errors**: `TreatWarningsAsErrors=true` globally. Any new warning fails the build.
- **Centralized package versions**: add NuGet versions only in [Directory.Packages.props](../Directory.Packages.props), never in individual `.csproj` files.
- **Sealed by default**: endpoints, handlers, `DbContext`s, and test classes should be `sealed`.
- **snake_case in PostgreSQL**: tables/columns are snake_case via `UseSnakeCaseNamingConvention()`. Match that in any raw SQL.
- **Frontend uses Bun, not pnpm/npm**: `src/Clients/` is managed by Bun (`bun@1.3.x`, `bun.lock`). Use `bun install`/`bun run`; running `pnpm`/`npm`/`yarn` creates a conflicting lockfile.
- **AppHost restart**: changes to `AppHost.cs` require restarting the AppHost (`aspire start`); other code hot-reloads.
- **Test project naming**: must end in `.UnitTests`, `.ContractTests`, or `.IntegrationTests` to be auto-detected.
- **Never modify** `global.json` or `NuGet.config` unless explicitly asked.

## Coding Standards

- Use latest C# 14 features
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
- Wolverine Inbox/Outbox entities registered in `OnModelCreating`
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

## See Also

- [CLAUDE.md](../CLAUDE.md) — full agent operating guide (Aspire flow, feature/endpoint templates, infra notes)
- [.github/CONTRIBUTING.md](./CONTRIBUTING.md) — contribution workflow, integration-event & proto standards, PR process
- [.github/instructions/](./instructions/) — language/tooling rules auto-applied via `applyTo` (C#, Next.js, Markdown, GitHub Actions, Context7)
- [.github/agents/](./agents/) — specialized subagents (`.NET Expert`, `Next.js Expert`, `Code Reviewer`, `Debug`, Spec Kit chain)
- [.agents/skills/](../.agents/skills/) — on-demand skills (Aspire, Turborepo, TUnit, EventCatalog authoring, React best practices)

<!-- SPECKIT START -->

For additional context about technologies to be used, project structure,
shell commands, and other important information, read the current plan

<!-- SPECKIT END -->
