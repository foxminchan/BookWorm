# BookWorm Constitution

## Core Principles

### I. Domain-Driven Design (NON-NEGOTIABLE)

Every service follows Domain-Driven Design principles with clear domain boundaries. Services must maintain their own domain models, use ubiquitous language, and implement proper aggregates. Domain logic must be isolated from infrastructure concerns and remain testable independently. Cross-service communication occurs only through well-defined integration events using the `BookWorm.Contracts` namespace.

### II. Microservices Architecture & Service Independence

Each service is independently deployable and owns its data. Services communicate through gRPC for synchronous operations and MassTransit/RabbitMQ for asynchronous event-driven communication. No direct database access between services. Service boundaries align with domain boundaries and business capabilities. Services include: Catalog, Basket, Ordering, Rating, Chat, Finance, Notification, and Scheduler. Frontend applications (Next.js Backoffice and Storefront) consume backend services through an API Gateway proxy and maintain clear separation of concerns.

### III. Test-First Development (NON-NEGOTIABLE)

100% business logic coverage is mandatory. Tests use **TUnit** framework with Microsoft Testing Platform (defined in `global.json`), not xUnit/NUnit/MSTest. Test projects follow naming: `*.UnitTests`, `*.ContractTests`, `*.IntegrationTests`. Unit tests use Moq for mocking, Bogus for test data generation, and Shouldly for assertions. Tests written using `Given_When_Then` naming pattern before implementation. Contract tests use Verify.TUnit for snapshot testing with MassTransit integration events. All domain and application logic must have comprehensive unit tests. Test projects include crash dumps, hang dumps, and TRX reporting via Microsoft.Testing.Extensions. Architecture tests use TngTech.ArchUnitNET.TUnit to validate architectural boundaries. Tests must be fast, independent, and deterministic.

### IV. Clean Architecture & Vertical Slice

Services follow Clean Architecture with clear separation between Domain (aggregates, entities, events, exceptions, value objects), Features (vertical slices with commands/queries), Infrastructure (persistence, integrations), and API layers. Implement Vertical Slice Architecture for feature organization under `/Features` folder. Use CQRS with `Mediator` library (source generator-based, not Mediator) for command/query separation. Custom `IEndpoint<TResult, TRequest>` interfaces from `BookWorm.Chassis` provide structured endpoint mapping via ASP.NET Core Minimal APIs. Domain events (inheriting from `IDomainEvent` in SharedKernel) handle side effects and cross-cutting concerns, dispatched via `IDomainEventDispatcher`.

### V. .NET 10 & Modern C# Standards

Use .NET 10 SDK with C# preview language features (`LangVersion=preview` in Directory.Build.props). Follow `.editorconfig` formatting rules with `TreatWarningsAsErrors=true`. Use file-scoped namespaces, primary constructors for immutable properties, expression-bodied members, pattern matching, and `is null`/`is not null` checks. Trust nullable reference type annotations and avoid unnecessary null checks. All projects use `ImplicitUsings` and target `net10.0`.

## Technology & Architecture Standards

### Cloud-Native with Aspire

All services orchestrated via `.NET Aspire` AppHost (`BookWorm.AppHost`) with service discovery, health checks, and observability built-in. Services reference `BookWorm.ServiceDefaults` for consistent telemetry, OpenTelemetry, API specifications (OpenAPI/AsyncAPI), and Kestrel configuration. Infrastructure includes Azure PostgreSQL Flexible Server (local containers for dev), Azure Managed Redis, Qdrant vector database, RabbitMQ (with MassTransit), Azure Storage (blob containers), and OpenAI models. Support containerization with OCI labels and Azure Container Apps provisioning. Implement health checks via gRPC health service and HTTP endpoints. Services use `Microsoft.Extensions.Caching.Hybrid` (via Chassis) for distributed caching with Redis backing. Keycloak provides authentication/authorization.

### Frontend Architecture & Technology Stack

**Monorepo Structure**: Frontend applications are organized as a Turbo-powered pnpm workspace monorepo located in `src/Clients`. Uses shared packages for code reuse including `api-client`, `api-hooks`, `ui`, `types`, `utils`, `validations`, `mocks`, and centralized `eslint-config` and `typescript-config`. All applications use TypeScript 5.7+, Node.js 25+, and follow consistent build/dev workflows via Turbo.

**Admin Backoffice** (`apps/backoffice`): Built with Next.js 16 and React 19 for administrative operations. Implements role-based access control, comprehensive data management interfaces, and real-time dashboards. Uses Tailwind CSS v4 with PostCSS, next-themes for theming, and Lucide React for icons. Must follow component-based architecture with proper state management and validation. Leverages shared UI components from `@workspace/ui` package.

**Storefront Website** (`apps/storefront`): Built with Next.js 16 and React 19 for optimal user experience and SEO. Implements server-side rendering, static generation where appropriate, and progressive web app capabilities. Uses Radix UI primitives for accessible components, TanStack Query for data fetching, TanStack Table for data grids, TanStack Form for form management, and Better Auth for authentication. Integrates with shared workspace packages for consistent API consumption and type safety. Must maintain responsive design using Tailwind CSS v4, accessibility standards (WCAG 2.1), and performance optimization (Core Web Vitals).

### Event-Driven Architecture

Use **MassTransit** with RabbitMQ transport for reliable message delivery. Implement outbox/inbox patterns via MassTransit.EntityFrameworkCore integration for transactional messaging. Integration events live in `/IntegrationEvents` folders within each service and use `BookWorm.Contracts` namespaces. All integration events must follow naming convention `[Action][Entity]IntegrationEvent` (e.g., `BookCreatedIntegrationEvent`) and include minimal necessary data. Domain events (within aggregates) are distinct from integration events and follow `[Entity][Action]Event` pattern (e.g., `FeedbackCreatedEvent`). Contract tests verify integration event schemas using Verify.MassTransit snapshot testing.

### AI & Modern Tooling Integration

Integrate AI components using **Microsoft Agents AI Framework** for multi-agent workflows. Chat and Rating services implement agent-based features with Dev UI enabled (`/dev/agent` endpoints). Model Context Protocol (MCP) server implemented in `BookWorm.McpTools` project with ASP.NET Core hosting (`ModelContextProtocol.AspNetCore`). Enable agent-to-agent communication via A2A Protocol (`Microsoft.Agents.AI.A2A`, `Microsoft.Agents.AI.Workflows`). Use Azure OpenAI (GPT-4o-mini for chat, text-embedding-3-large for embeddings) via Semantic Kernel with Qdrant vector store for RAG patterns. MCP server exposes catalog and other service capabilities as tools. Frontend applications integrate AI features through service endpoints.

### API Gateway & Frontend Integration

API Gateway implemented via custom proxy in AppHost (`.AddApiGatewayProxy()`) routing to backend services (Catalog, Basket, Ordering, Rating, Chat). Services expose REST APIs with ASP.NET Core Minimal APIs mapped through custom `IEndpoint` interfaces. Support Keycloak-based authentication/authorization with `KeycloakTokenIntrospectionMiddleware`. Maintain consistent API versioning via `Asp.Versioning.Mvc.ApiExplorer` with `HasApiVersion(new(1, 0))`. Services include OpenAPI specifications. gRPC used for inter-service synchronous communication (e.g., `BookService`). Frontend applications consume services through gateway with typed API clients in `@workspace/api-client` and React hooks in `@workspace/api-hooks`.

## Development Workflow & Quality Gates

### Code Quality & Standards

Follow DDD principles strictly. Domain models live in `/Domain` with aggregates implementing `IAggregateRoot`, entities inheriting from `Entity` or `AuditableEntity`, and value objects as records/classes. Use explicit type declarations when type isn't obvious. Implement proper error handling with domain-specific exceptions (e.g., `RatingDomainException`) inheriting from base exception classes. Use Repository pattern (interfaces in Domain, implementations in Infrastructure with EF Core) for data access. Specification pattern (from Chassis) for complex queries. Validation via FluentValidation integrated with endpoint pipeline. Services use `Scrutor` for assembly scanning and convention-based DI registration. PostgreSQL databases use snake_case naming via `EFCore.NamingConventions`. Rate limiting, CORS, HSTS, and security middleware configured in `Program.cs`.

**Frontend Code Standards**: All frontend code must use TypeScript with strict type checking. Follow React 19 best practices including proper hooks usage, component composition, and performance optimization with React Compiler. Next.js applications must use App Router, implement proper error boundaries, leverage Server Components where appropriate, and maintain consistent folder structure (`app/`, `components/`, `hooks/`, `lib/`). Shared packages must export properly typed APIs. Use TanStack Query for server state, React Context for client state when needed, and Zod (via validations package) for runtime validation. Both applications must include comprehensive unit tests (Vitest/Jest) and integration tests (Playwright/Testing Library).

### Pull Request Requirements

All PRs require maintainer approval and passing CI/CD checks including SonarQube analysis and Snyk security scans. Include comprehensive unit tests for new features. Update documentation for functionality changes. Link related issues using keywords (Fixes #123). Address reviewer feedback promptly.

### Documentation & API Standards

**Documentation Infrastructure**: Project documentation maintained in `docs/` folder with two primary systems:

- **EventCatalog** (`docs/eventcatalog/`): Living documentation for event-driven architecture including all integration events, domain events, services, and message flows. OpenAPI specifications stored in `openapi-files/` subdirectory. EventCatalog visualizes service interactions, event schemas, and AsyncAPI specifications. All integration events must be documented here with examples and versioning.

- **Docusaurus** (`docs/docusaurus/`): Comprehensive project documentation including architecture decisions, development guides, deployment procedures, and API references. Docusaurus provides versioned documentation with search capabilities and structured navigation. Technical documentation, tutorials, and ADRs (Architecture Decision Records) live here.

**API Standards**: Use OpenAPI for REST APIs and AsyncAPI for event-driven endpoints. All services must generate OpenAPI specs via Swashbuckle/NSwag. AsyncAPI documents for MassTransit consumers/producers must be maintained in EventCatalog. Include code comments for complex logic using XML documentation. Provide clear examples for API changes. API contracts (request/response DTOs) must include validation attributes and descriptive properties.

**Frontend Documentation**: Document component APIs in shared `@workspace/ui` package using JSDoc/TSDoc. Maintain component documentation for both Next.js applications with usage examples. Document state management patterns (TanStack Query, React Context), form handling (TanStack Form), and integration patterns with backend services via `@workspace/api-client` and `@workspace/api-hooks`. Include deployment guides for both admin and storefront applications (Vercel/Azure/Docker). Document authentication flows (Better Auth integration), API consumption patterns, monorepo workspace conventions, and shared package development. Maintain up-to-date README files in each workspace package explaining purpose, API surface, and usage examples.

## Governance

This constitution supersedes all other development practices and guidelines. All PRs and code reviews must verify compliance with these principles. Complexity must be justified and aligned with business value. Architecture violations detected by tests must be resolved before merging.

For detailed development guidance, refer to `AGENTS.md` and `.github/CONTRIBUTING.md`. Integration event namespaces must never be modified as it disrupts the messaging system routing.

**Version**: 1.0.0 | **Ratified**: 2025-09-27 | **Last Amended**: 2025-12-28
