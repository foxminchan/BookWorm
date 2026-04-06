<!--
  Sync Impact Report
  ==================
  Version change: N/A (initial) → 1.0.0
  Modified principles: N/A (initial population)
  Added sections:
    - Core Principles (7 principles)
    - Technology Stack Constraints
    - Development Workflow
    - Governance
  Removed sections: None
  Templates requiring updates:
    - .specify/templates/plan-template.md ✅ no update needed (dynamic reference)
    - .specify/templates/spec-template.md ✅ no update needed (generic placeholders)
    - .specify/templates/tasks-template.md ✅ no update needed (generic placeholders)
    - .specify/templates/checklist-template.md ✅ no update needed
    - .specify/templates/agent-file-template.md ✅ no update needed
  Follow-up TODOs: None
-->

# BookWorm Constitution

## Core Principles

### I. Domain-Driven Design

Every service MUST be modeled around bounded contexts with explicit
aggregate boundaries. Business logic MUST reside in the domain layer,
not in application services or handlers. Domain events MUST capture
all business-significant state changes and serve as the mechanism for
cross-context communication.

- Ubiquitous language MUST be consistent across code, documentation,
  and team communication within each bounded context.
- Aggregates MUST enforce transactional consistency boundaries.
- Value objects MUST be used for concepts with no identity (mapped
  via `OwnsOne()` in EF Core).
- Soft deletes via `HasQueryFilter(x => !x.IsDeleted)` MUST be
  applied where domain requires logical deletion.

### II. Vertical Slice Architecture with CQRS

Features MUST be organized as self-contained vertical slices under
`Features/{FeatureName}/` within each service. Each slice contains
its command/query, handler, and endpoint.

- Commands MUST implement `ICommand<T>`; queries MUST implement
  `IQuery<T>` using `Mediator.SourceGenerator` (NOT MediatR).
- Endpoints MUST implement `IEndpoint<TResult, TRequest>` from
  BookWorm.Chassis with `MapEndpoint()` and `HandleAsync()`.
- Endpoints MUST chain `.ProducesGet<T>()`, `.MapToApiVersion()`,
  and `.RequireAuthorization()` as appropriate.
- No manual route registration — endpoints are auto-discovered.

### III. Test Discipline

Every service MUST maintain unit tests, contract tests, and where
applicable integration tests. Test quality gates MUST be enforced
in CI.

- Test naming: `GivenCondition_WhenAction_ThenExpectedResult()`.
- Test classes MUST be `sealed`.
- Test data MUST use Bogus Fakers; assertions MUST use Shouldly.
- Test projects follow `BookWorm.{Service}.{UnitTests|ContractTests|
IntegrationTests}` naming.
- Minimum 85% coverage for domain and application layers.
- Architecture tests in `tests/BookWorm.ArchTests/` MUST pass.

### IV. Event-Driven Microservices

Inter-service communication MUST use MassTransit with Kafka.
Transactional consistency MUST be maintained via outbox/inbox
patterns. Saga orchestration MUST be used for multi-step
business processes.

- Outbox/Inbox entities MUST be registered in `OnModelCreating`.
- Events MUST be documented in EventCatalog (`docs/eventcatalog/`).
- gRPC MUST be used for synchronous service-to-service calls
  where event-driven patterns are not appropriate.

### V. Code Quality as Law

The codebase MUST compile with zero warnings. Quality enforcement
is automated and non-negotiable.

- `TreatWarningsAsErrors=true` — the build MUST fail on any warning.
- All endpoint, handler, test, and DbContext classes MUST be `sealed`.
- Package versions MUST be centralized in `Directory.Packages.props`;
  individual `.csproj` files MUST NOT specify version numbers.
- C# 14 features (`LangVersion=preview`) MUST be used: file-scoped
  namespaces, primary constructors, pattern matching, switch
  expressions.
- `async`/`await` MUST be used end-to-end with `CancellationToken`
  propagation. No fire-and-forget.
- `global.json` and `NuGet.config` MUST NOT be modified unless
  explicitly requested.

### VI. Security by Design

Security MUST be enforced at system boundaries and verified
continuously.

- OWASP Top 10 vulnerabilities MUST be addressed in all code.
- Authentication via Keycloak with token introspection; endpoints
  MUST use `.RequireAuthorization()` where access control applies.
- Secrets MUST NOT be committed; use User Secrets for local
  development and environment variables in CI/deployment.
- PII MUST be scrubbed from logs.
- Inputs MUST be validated at service boundaries.
- All public APIs MUST have XML doc comments.

### VII. Simplicity and YAGNI

Complexity MUST be justified. The default is the simplest approach
that satisfies the requirement.

- Only make changes that are directly requested or clearly necessary.
- Do not add features, refactor code, or make "improvements" beyond
  what was asked.
- Do not add error handling for scenarios that cannot occur.
- Do not create helpers or abstractions for one-time operations.
- Start simple; optimize only when measured to help.

## Technology Stack Constraints

The following technology choices are binding and MUST NOT be changed
without a constitutional amendment:

- **Backend**: C# 14, .NET 10, ASP.NET Core Minimal APIs
- **ORM**: EF Core + PostgreSQL with snake_case naming convention
  (`UseSnakeCaseNamingConvention()`) and UUID v7 primary keys
- **Frontend**: TypeScript 5.7+, Next.js 16, React 19, pnpm + Turbo
  monorepo
- **CQRS**: `Mediator.SourceGenerator` (source generator-based)
- **Messaging**: MassTransit with Kafka
- **Auth**: Keycloak (Authorization Code Flow with PKCE for users;
  Token Exchange for service-to-service)
- **Caching**: HybridCache (distributed + local tiers)
- **Testing**: TUnit, Moq, Bogus, Shouldly, Verify.TUnit
- **AI**: Microsoft Agents AI Framework, Semantic Kernel, MCP server
- **Orchestration**: Aspire AppHost
- **CI/CD**: GitHub Actions
- **Documentation**: EventCatalog, Docusaurus, OpenAPI, AsyncAPI

## Development Workflow

All contributions MUST follow these workflow gates:

1. **Branch & Feature**: Work on a feature branch; one feature per
   branch aligned to a vertical slice.
2. **Build**: `dotnet build` MUST pass with zero warnings. Frontend
   builds via `pnpm run build` in `src/Clients/`.
3. **Test**: All existing tests MUST pass. New features MUST include
   corresponding unit tests at minimum.
4. **Format**: Code MUST be formatted (`just format`) before commit.
5. **Review**: All PRs MUST verify compliance with this constitution.
   Complexity MUST be justified in the PR description.
6. **CI**: GitHub Actions pipelines MUST pass (backend CI + frontend
   CI + SonarCloud quality gate).
7. **Documentation**: Architecture changes MUST be reflected in
   EventCatalog. API changes MUST update OpenAPI specs.

Runtime development guidance is maintained in:

- `AGENTS.md` (agent workflow instructions)
- `.github/copilot-instructions.md` (coding standards)
- `.github/instructions/` (domain-specific instruction files)

## Governance

This constitution supersedes all other development practices and
conventions for the BookWorm project. In case of conflict between
this document and any other guidance file, this constitution
prevails.

- **Amendments**: Any change to this constitution MUST be documented
  with a version bump, rationale, and migration plan if principles
  are removed or redefined.
- **Versioning**: Constitution versions follow semantic versioning:
  - MAJOR: Backward-incompatible principle removals or redefinitions
  - MINOR: New principle/section added or materially expanded
  - PATCH: Clarifications, wording, or non-semantic refinements
- **Compliance Review**: All PRs and code reviews MUST verify
  adherence to these principles. Violations MUST be resolved before
  merge.
- **Enforcement**: CI pipelines enforce build quality (warnings as
  errors, test gates, format checks). Manual review enforces
  architectural and design principles.

**Version**: 1.0.0 | **Ratified**: 2026-04-06 | **Last Amended**: 2026-04-06
