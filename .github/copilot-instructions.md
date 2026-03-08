# BookWorm — Copilot Instructions

## Project Identity

BookWorm is a .NET 10 microservices bookstore using Aspire orchestration, DDD with Clean Architecture, and event-driven patterns (MassTransit/RabbitMQ).

## Tech Stack

- **Backend**: C# 14 (`LangVersion=preview`), ASP.NET Core Minimal APIs, EF Core + PostgreSQL (snake_case)
- **Frontend**: TypeScript 5.7+, Next.js 16, React 19, pnpm + Turbo monorepo
- **CQRS**: Mediator library (source generator-based, NOT MediatR)
- **Testing**: TUnit, Moq, Bogus, Shouldly, Verify.TUnit
- **Messaging**: MassTransit with RabbitMQ (outbox/inbox patterns)
- **AI**: Microsoft Agents AI Framework, Semantic Kernel, MCP server
- **Auth**: Keycloak with token introspection

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

## Key Locations

- AppHost: `src/Aspire/BookWorm.AppHost/AppHost.cs`
- Services: `src/Services/{Name}/BookWorm.{Name}/`
- Frontend: `src/Clients/` (Turbo monorepo)
- Shared: `src/BuildingBlocks/` (Chassis, Constants, SharedKernel)
