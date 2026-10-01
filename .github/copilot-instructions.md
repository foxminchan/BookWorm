# BookWorm — Copilot Instructions

## Project identity

BookWorm is a .NET 10 microservices application built around Aspire, DDD, and event-driven integration patterns. The codebase emphasizes Vertical Slice Architecture, CQRS, and clean service boundaries.

## Workflow

- Prefer the repo task runner in [mise.toml](../mise.toml) instead of raw `dotnet` or `bun` commands.
- Use the smallest relevant validation command for routine changes.
- Do not start the Aspire AppHost unless the user explicitly requests runtime validation.
- When AppHost changes are involved, restart only when validation is requested or the app is already running.

## Commands

- `mise run restore` — restore .NET packages and tools
- `mise run build` — build the solution
- `mise run test` — run the test suite
- `mise run format` — format C#, frontend, docs, and local tooling assets
- `mise run run` — only for explicit runtime validation with Aspire

## Architecture and conventions

- Use Vertical Slice Architecture and DDD boundaries; keep business logic near the feature and domain.
- Use `Mediator.SourceGenerator`; do not introduce `MediatR`.
- Keep endpoints, handlers, DbContexts, and tests `sealed` by default.
- Keep `async`/`await` and `CancellationToken` propagation consistent across service boundaries.
- Prefer modern C# 14 idioms, file-scoped namespaces, and pattern matching.
- Treat warnings as errors; the build is strict.
- Keep PostgreSQL naming and raw SQL in `snake_case`.
- Centralize package versions in [Directory.Packages.props](../Directory.Packages.props); do not add NuGet versions to individual project files.
- The frontend is a Bun-based monorepo under [src/Clients](../src/Clients); use `bun run` rather than `npm` or `pnpm`.
- Keep secrets out of source control; prefer user secrets or environment variables.

## Project structure

- AppHost: [src/Aspire/BookWorm.AppHost/AppHost.cs](../src/Aspire/BookWorm.AppHost/AppHost.cs)
- Services: [src/Services](../src/Services)
- Shared libraries: [src/BuildingBlocks](../src/BuildingBlocks)
- Frontend: [src/Clients](../src/Clients)
- Integration components: [src/Integrations](../src/Integrations)
- Tests: [tests](../tests)

## Common pitfalls

- `Mediator.SourceGenerator` is not `MediatR`.
- `snake_case` is required for PostgreSQL schema names and raw SQL.
- `AppHost.cs` changes often require a restart in a running environment.
- Frontend lockfiles are Bun-managed, not npm or pnpm.
- Test projects should end with `.UnitTests`, `.ContractTests`, or `.IntegrationTests`.
- Never modify [global.json](../global.json) or [NuGet.config](../NuGet.config) unless explicitly asked.

## References

- [README.md](../README.md)
- [.github/CONTRIBUTING.md](./CONTRIBUTING.md)
- [.github/instructions](./instructions)
- [.agents/skills](../.agents/skills)
- [docs/docusaurus](../docs/docusaurus)
- [docs/eventcatalog](../docs/eventcatalog)

## Official docs to prefer

- https://aspire.dev
- https://learn.microsoft.com/dotnet/aspire
- https://nuget.org
