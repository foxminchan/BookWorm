# Agent Instructions

## Working style

- Prefer the repo task runner in [mise.toml](mise.toml) over raw `dotnet` or `bun` commands.
- Do not start the Aspire AppHost unless the user explicitly asks for runtime validation.
- For routine changes, use the smallest relevant build, test, lint, or type-check command.
- If AppHost code changes, restart only when validation is requested or the app is already running.

## Quick start

- Fresh clone: `mise install`, then `mise run prepare`
- Restore: `mise run restore`
- Build: `mise run build`
- Test: `mise run test`
- Format: `mise run format`
- Run the app: `mise run run` only when explicitly requested

## Project shape

- .NET 10 / C# 14 microservices with Aspire orchestration.
- AppHost: [src/Aspire/BookWorm.AppHost/AppHost.cs](src/Aspire/BookWorm.AppHost/AppHost.cs)
- Shared libraries: [src/BuildingBlocks](src/BuildingBlocks)
- Frontend: [src/Clients](src/Clients) (Bun + Turbo + Next.js 16 / React 19)
- Services: [src/Services](src/Services)
- Integrations: [src/Integrations](src/Integrations)
- Tests: [tests](tests)
- Documentation: [README.md](README.md), [docs/docusaurus](docs/docusaurus), [docs/eventcatalog](docs/eventcatalog)

## Architecture and coding conventions

- Follow Vertical Slice Architecture and DDD boundaries.
- Use the source-generated CQRS model from `Mediator.SourceGenerator`; do not add `MediatR`.
- Keep endpoints, handlers, DbContexts, and tests `sealed` by default.
- Keep asynchronous flows fully `async`/`await` with `CancellationToken` propagation.
- Treat warnings as errors; use file-scoped namespaces and modern C# features where appropriate.
- Keep PostgreSQL identifiers and raw SQL in `snake_case`.
- Centralize package versions in [Directory.Packages.props](Directory.Packages.props); avoid pinning versions in individual project files.
- Frontend is Bun-based; use `bun run` from [src/Clients](src/Clients), not `npm` or `pnpm`.
- Keep secrets out of source control; use user secrets/local environment variables instead.

## Common patterns to preserve

- Event-driven services use WolverineFx + Kafka with outbox/inbox patterns.
- Auth is handled via Keycloak; apply authorization at endpoint boundaries.
- Caching uses FusionCache; service boundaries should remain consistent with the existing architecture.
- API versioning is explicit; follow the existing `ApiVersions.V1` patterns.

## References

- [README.md](README.md)
- [.github/CONTRIBUTING.md](.github/CONTRIBUTING.md)
- [.github/copilot-instructions.md](.github/copilot-instructions.md)
- [docs/docusaurus](docs/docusaurus)
- [docs/eventcatalog](docs/eventcatalog)
- [.agents/skills](.agents/skills)

## Official docs to prefer

- https://aspire.dev
- https://learn.microsoft.com/dotnet/aspire
- https://nuget.org
