# BookWorm — Copilot instructions

## System overview

BookWorm is a microservices-based bookstore system built around .NET Aspire for service orchestration, local developer experience, and cloud-ready composition. It combines:

- **8 Microservices** with clear bounded contexts: Catalog, Basket, Ordering, Rating, Chat, Finance, Notification, and Scheduler.
- **Microsoft Agents AI Framework** integrated as first-class components for content enrichment, personalized recommendations, chat functionality, and RAG patterns.
- **Next.js 16 + React 19 frontend applications** in a Turbo monorepo: Backoffice (admin) and Storefront (customer-facing).
- A unified developer experience via Aspire's AppHost with service discovery, health checks, and observability.
- Custom API Gateway proxy routing to backend services with Keycloak authentication.
- Automated testing with **TUnit** framework, Moq for mocking, Bogus for test data, and Shouldly for assertions.

Goals:
- Demonstrate modern .NET 10 microservices patterns with Aspire orchestration.
- Show DDD with Clean Architecture and Vertical Slice organization.
- Integrate AI agents using MCP (Model Context Protocol) for standardized AI tooling.
- Provide end-to-end reference for event-driven architecture with MassTransit/RabbitMQ.

## Tech stack and key dependencies

- **Runtime**: .NET 10 with C# preview language features (`LangVersion=preview`).
- **Orchestration**: .NET Aspire (AppHost in `BookWorm.AppHost`, ServiceDefaults in `BookWorm.ServiceDefaults`).
- **Application code**: C# (backend services), TypeScript 5.7+ (frontend in Turbo monorepo), PowerShell for scripts, Justfile for dev ergonomics.
- **Backend framework**: ASP.NET Core Minimal APIs with custom `IEndpoint<TResult, TRequest>` pattern from BookWorm.Chassis.
- **CQRS**: Mediator library (source generator-based, NOT MediatR) for command/query separation.
- **Testing**: **TUnit** with Microsoft Testing Platform, **Moq**, **Bogus**, **Shouldly**, **Verify.TUnit** for contract tests, **TngTech.ArchUnitNET.TUnit** for architecture tests.
- **Database**: Azure PostgreSQL Flexible Server with EF Core and `EFCore.NamingConventions` (snake_case).
- **Messaging**: **MassTransit with RabbitMQ** for event-driven communication with outbox/inbox patterns.
- **Caching**: `Microsoft.Extensions.Caching.Hybrid` with Azure Managed Redis.
- **Storage**: Azure Storage (blob containers) for Catalog service.
- **Vector DB**: Qdrant for embeddings and RAG patterns.
- **Authentication**: Keycloak with token introspection middleware.
- **Observability**: Aspire-enabled OpenTelemetry, health endpoints, distributed tracing.
- **AI**: Microsoft Agents AI Framework with Azure OpenAI (GPT-4o-mini, text-embedding-3-large), Semantic Kernel, MCP server (`BookWorm.McpTools`), A2A Protocol.
- **Frontend**: Next.js 16, React 19, TanStack Query/Table/Form, Radix UI, Tailwind CSS v4, Better Auth, pnpm + Turbo.

Key locations:
- AppHost: `src/Aspire/BookWorm.AppHost/AppHost.cs`
- Services: `src/Services/{ServiceName}/BookWorm.{ServiceName}/`
- Frontend: `src/Clients/` (monorepo with `apps/` and `packages/`)
- Shared libraries: `src/BuildingBlocks/` (Chassis, Constants, SharedKernel)
- MCP Tools: `src/Integrations/BookWorm.McpTools/`

## Local development and onboarding

Prerequisites:
- .NET SDK (per global.json if present; do not modify global.json).
- Node.js + bun (if a frontend exists).
- Dev dependencies used by AppHost (e.g., Docker, local emulators) if required by resources.

Recommended flow:
1) Restore and build
   - Run “just” commands if a Justfile exists: `just bootstrap` or `just build`.
   - Or use `dotnet restore` then `dotnet build` at the solution root.

2) Run the system
   - Prefer launching the Aspire AppHost: `dotnet run` inside the AppHost project.
   - Inspect the Aspire dashboard URL from console output for health, traces, and service links.

3) Frontend (if present)
   - From the frontend folder: `pnpm i`, then `pnpm run dev`/`build`.
   - Ensure API base URLs match local dev settings or proxy via API Gateway/BFF.

4) Configuration and secrets
   - Use User Secrets or environment variables for API keys (especially AI model providers).
   - Never commit secrets. Check `README`/scripts for local `.env` conventions if provided.

5) Tests
   - Run `dotnet test` from the solution root or specific test projects.
   - Follow TUnit patterns; use Moq for mocks.

## Common tasks (Justfile commands)

- `just restore`: Restore .NET and pnpm dependencies
- `just build`: Compile all backend projects and frontend apps
- `just run`: Start Aspire AppHost with all services and frontend
- `just test`: Run all TUnit tests with coverage
- `just format`: Apply formatting (.NET + frontend)
- `just clean`: Clean build artifacts
- `just trust`: Trust .NET dev certificates

If no Justfile: Use equivalent `dotnet` and `pnpm` commands directly.

## Security and compliance

- Never commit secrets or API keys. Prefer User Secrets/local env for dev; key vaults for cloud.
- Validate inputs at service boundaries.
- Consider rate limiting and circuit breakers for external AI/model calls.
- Scrub PII in logs; redact prompts/responses as needed.
