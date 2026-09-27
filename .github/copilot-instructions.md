# BookWorm — Copilot Instructions

## Project Identity

BookWorm is a .NET 10 microservices bookstore using Aspire orchestration, DDD with Clean Architecture, and event-driven patterns (WolverineFx/Kafka).

## Tech Stack

- **Backend**: C# 14 (`LangVersion=preview`), .NET 10, ASP.NET Core Minimal APIs, EF Core 10 + PostgreSQL (snake_case)
- **Frontend**: TypeScript 7.0, Next.js 16.3, React 19, Bun + Turbo monorepo (versions and Node requirements in `src/Clients/package.json`)
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
- `mise run run` — start the Aspire AppHost only when explicitly requested by the user
- `mise run format` — format C# (CSharpier), frontend, EventCatalog, Docusaurus, k6, Keycloakify
- `mise run prepare` — post-clone setup (restore + git hooks)

For routine changes, use the smallest relevant build, test, lint, or type-check command. Read the affected package scripts before selecting frontend checks. Do not start the Aspire AppHost unless explicitly requested. If Aspire validation is requested, use its CLI/MCP tools to inspect resources and diagnose failures.

## Common Pitfalls

- **Mediator ≠ MediatR**: this repo uses `Mediator.SourceGenerator` (source-generator-based). Same-looking interfaces, different package — do not add MediatR.
- **Warnings = errors**: `TreatWarningsAsErrors=true` globally. Any new warning fails the build.
- **Centralized package versions**: add NuGet versions only in [Directory.Packages.props](../Directory.Packages.props), never in individual `.csproj` files.
- **Sealed by default**: endpoints, handlers, `DbContext`s, and test classes should be `sealed`.
- **snake_case in PostgreSQL**: tables/columns are snake_case via `UseSnakeCaseNamingConvention()`. Match that in any raw SQL.
- **Frontend uses Bun, not pnpm/npm**: `src/Clients/` is managed by Bun (`packageManager` in `src/Clients/package.json`, `bun.lock`). Use `bun install`/`bun run`; do not use `pnpm` or `npm` directly.
- **AppHost restart**: changes to `AppHost.cs` require a restart when the application is already running or the user requests Aspire validation.
- **Test project naming**: must end in `.UnitTests`, `.ContractTests`, or `.IntegrationTests` to be auto-detected.
- **Never modify** `global.json` or `NuGet.config` unless explicitly asked.

## Coding Standards

- Use latest C# 14 features
- Follow DDD aggregate boundaries; business logic belongs in the domain layer
- Use `async`/`await` end-to-end with `CancellationToken` propagation
- Prefer `var` when type is obvious; use pattern matching and switch expressions
- Apply file-scoped namespaces and primary constructors
- Never commit secrets or API keys; use User Secrets for local dev
- Validate inputs at service boundaries; scrub PII in logs
- All public APIs require XML doc comments

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

- [.github/CONTRIBUTING.md](./CONTRIBUTING.md) — contribution workflow, integration-event & proto standards, PR process
- [.github/instructions/](./instructions/) — language/tooling rules auto-applied via `applyTo` (C#, Next.js, Markdown, GitHub Actions, Context7)
- [.agents/skills/](../.agents/skills/) — on-demand skills (Aspire, Turborepo, TUnit, EventCatalog authoring, React best practices)
