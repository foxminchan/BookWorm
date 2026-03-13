# BookWorm Constitution

## Core Principles

### I. Code Quality

All source code MUST adhere to the following non-negotiable
standards:

- **C# 14 & latest language features**: Use file-scoped
  namespaces, primary constructors, pattern matching, switch
  expressions, and expression-bodied members. Never modify
  `global.json` or `NuGet.config` without explicit approval.
- **TypeScript 5.7+ strict mode**: All frontend code MUST
  compile under strict TypeScript with no `any` escape hatches
  unless justified and documented inline.
- **Formatting enforcement**: `.editorconfig` rules are
  authoritative for C#; ESLint and Prettier configurations are
  authoritative for TypeScript. Code that violates formatting
  MUST NOT be merged.
- **Nullable reference types**: Variables MUST be declared
  non-nullable by default. Use `is null` / `is not null` for
  null checks. Redundant null guards after annotation are
  prohibited.
- **Error handling**: Use precise exception types
  (`ArgumentException`, `InvalidOperationException`). Never
  throw or catch base `Exception`. Never swallow errors
  silently. Use `ArgumentNullException.ThrowIfNull()` for null
  guards.
- **Async discipline**: All async methods MUST end with
  `Async`, propagate `CancellationToken` end-to-end, and
  `await` every asynchronous call — no fire-and-forget.
  Use `ConfigureAwait(false)` in library code.
- **XML documentation**: All public APIs MUST have XML doc
  comments including `<summary>`, `<param>`, and `<returns>`.
  Include `<example>` blocks where the usage is non-obvious.
- **SOLID compliance**: Every class MUST have a single reason
  to change. Depend on abstractions, not concretions. No
  client is forced to depend on methods it does not use.

**Rationale**: Consistent, high-quality code reduces cognitive
load across the microservices boundary, minimizes defect rates,
and ensures the codebase remains maintainable as the team and
service count grow.

### II. Domain-Driven Design

All services MUST follow DDD and Clean Architecture principles:

- **Ubiquitous Language**: Business terminology MUST be
  consistent across code, documentation, API contracts, and
  event schemas. Renaming a domain concept requires updating
  all references.
- **Bounded Contexts**: Each microservice represents a single
  bounded context with well-defined responsibilities. Cross-
  context communication MUST occur through domain events via
  MassTransit/Kafka, never through shared databases.
- **Aggregate boundaries**: Aggregates enforce transactional
  consistency. Business logic MUST reside in the domain layer
  (entities, value objects, domain services) — not in
  application services, controllers, or infrastructure.
- **Rich Domain Models**: Anemic domain models are prohibited.
  Entities MUST encapsulate behavior and enforce invariants.
  Use domain events to capture business-significant state
  changes.
- **Layer separation**: Domain → Application → Infrastructure.
  The domain layer MUST NOT reference application or
  infrastructure concerns. Infrastructure adapters (EF Core,
  MassTransit, gRPC) MUST be isolated behind abstractions.
- **CQRS**: Commands and queries MUST be separated using the
  Mediator library (source-generator-based). Each command/
  query handler is a single-purpose unit.
- **Event-driven integration**: Outbox and inbox patterns are
  REQUIRED for reliable cross-service messaging. Saga patterns
  MUST be used for multi-step distributed workflows.

**Rationale**: DDD aligns the software model with business
reality, making the system easier to reason about, extend, and
operate at scale. Clean Architecture ensures testability and
independence from infrastructure choices.

### III. Testing Standards

Every feature MUST be accompanied by appropriate tests:

- **Unit tests**: REQUIRED for all domain logic and application
  layer handlers. Use TUnit as the test framework, Moq for
  mocking, Bogus for test data generation, and Shouldly for
  assertions. Verify.TUnit MUST be used for snapshot testing.
- **Contract tests**: REQUIRED for every service exposing or
  consuming an API. Contract test projects MUST exist for each
  service (`BookWorm.{Service}.ContractTests`).
- **Architecture tests**: The `BookWorm.ArchTests` project MUST
  validate layer dependency rules, naming conventions, and
  structural constraints. New services MUST be covered.
- **Integration tests**: REQUIRED for aggregate boundaries,
  repository implementations, and cross-service communication
  paths.
- **Frontend tests**: Component tests and unit tests MUST cover
  all React components and hooks. End-to-end tests use
  Playwright BDD for critical user journeys.
- **Coverage threshold**: Domain and application layers MUST
  maintain a minimum of 85% line coverage. Coverage regressions
  MUST NOT be merged.
- **Naming convention**: Test methods MUST follow the pattern
  `GivenCondition_WhenAction_ThenExpectedResult()`.
- **Test independence**: Each test MUST be independently
  executable. Tests MUST NOT depend on execution order or
  shared mutable state.

**Rationale**: A comprehensive, layered testing strategy catches
defects early, validates contract compatibility across services,
and provides confidence for continuous deployment in a
microservices architecture.

### IV. User Experience Consistency

All user-facing surfaces MUST deliver a cohesive, accessible
experience:

- **WCAG 2.1 AA compliance**: All frontends (StoreFront and
  BackOffice) MUST meet WCAG 2.1 Level AA. Semantic HTML, ARIA
  attributes, keyboard navigation, and sufficient color
  contrast are mandatory.
- **Design system adherence**: UI components MUST be sourced
  from or consistent with the shared component library in
  `BookWorm.StoreFront.Components`. Ad-hoc styling that
  diverges from the design system is prohibited.
- **Responsive design**: All pages MUST render correctly on
  viewports from 320px to 2560px wide. Mobile-first layout
  strategies are preferred.
- **Loading & error states**: Every data-fetching operation
  MUST display appropriate loading indicators and user-friendly
  error messages. Empty states MUST provide guidance or calls
  to action.
- **API consistency**: All REST endpoints MUST follow the
  `IEndpoint<TResult, TRequest>` pattern from
  `BookWorm.Chassis`. OpenAPI documentation is REQUIRED for
  every public endpoint. Error responses MUST use RFC 9457
  Problem Details format.
- **Internationalization readiness**: User-facing strings MUST
  NOT be hardcoded. Text content MUST be externalizable even
  if only one locale is currently supported.
- **Performance perception**: Initial page load MUST display
  meaningful content within 1.5 seconds on a 4G connection.
  Optimistic UI updates are preferred for mutation operations.

**Rationale**: Users interact with multiple BookWorm surfaces
(storefront, backoffice, chatbot). A consistent, accessible,
and responsive experience builds trust and reduces support
burden.

### V. Performance Requirements

All services MUST meet measurable performance targets:

- **API response times**: P95 latency for read endpoints MUST
  be ≤ 200ms. P95 latency for write endpoints MUST be ≤ 500ms.
  Endpoints exceeding these thresholds MUST be documented with
  justification.
- **Database efficiency**: N+1 query patterns are prohibited.
  All EF Core queries MUST use explicit loading strategies
  (eager or split queries). PostgreSQL indexes MUST be defined
  for all frequently filtered columns.
- **Caching strategy**: HybridCache MUST be used for
  frequently-read, infrequently-mutated data. Cache
  invalidation strategies MUST be explicit and documented.
  Cache-aside patterns MUST NOT introduce stale-read windows
  exceeding 5 seconds for user-facing data.
- **Async I/O**: All I/O operations (database, HTTP, message
  bus, file system) MUST use `async`/`await`. Blocking calls
  (`Task.Result`, `Task.Wait()`, `.GetAwaiter().GetResult()`)
  are prohibited in request-handling paths.
- **Memory allocation**: Avoid unnecessary allocations in hot
  paths. Use `Span<T>`, `ReadOnlySpan<T>`, and array pooling
  where measured to help. Seal classes that are not designed
  for inheritance. Prefer `readonly struct` for small value
  types.
- **Load testing**: Critical user journeys MUST be validated
  with k6 load tests. Services MUST handle their documented
  throughput targets without degradation.
- **Frontend performance**: Core Web Vitals targets: LCP
  ≤ 2.5s, INP ≤ 200ms, CLS ≤ 0.1. Bundle sizes MUST be
  monitored; regressions exceeding 10% MUST be justified.

**Rationale**: A bookstore handling concurrent users across
catalog browsing, ordering, and AI-powered chat must deliver
consistent low-latency responses. Explicit targets make
performance a first-class, measurable concern rather than an
afterthought.

## Security & Observability Standards

These cross-cutting concerns apply to all services and
frontends:

- **Authentication & Authorization**: Keycloak with
  Authorization Code Flow + PKCE for user auth, Token Exchange
  for service-to-service auth. Authorization MUST be enforced
  at the aggregate level. No endpoint may be publicly
  accessible without explicit opt-in.
- **Secrets management**: Secrets MUST NOT appear in source
  code, logs, or configuration files. Use User Secrets for
  local development and environment variables for deployed
  environments.
- **Input validation**: All inputs MUST be validated at service
  boundaries. Use FluentValidation or equivalent. PII MUST be
  scrubbed from all log output.
- **Structured logging**: All services MUST emit structured
  logs compatible with the Aspire dashboard. Log levels MUST
  follow: `Trace` for diagnostics, `Information` for business
  events, `Warning` for recoverable issues, `Error` for
  failures requiring attention.
- **Health checks**: Every service MUST expose health check
  endpoints compatible with the Aspire orchestrator. Dependency
  health (database, message bus, external APIs) MUST be
  included.
- **Distributed tracing**: OpenTelemetry traces and metrics
  MUST propagate across service boundaries. Trace context MUST
  be preserved through MassTransit message headers.

## Development Workflow

All contributors MUST follow this workflow:

- **Branching**: Feature branches MUST follow the naming
  convention `###-feature-name`. All work MUST be done on
  feature branches; direct commits to `main` are prohibited.
- **Pull requests**: Every PR MUST pass CI (build, test, lint,
  format) before merge. PRs MUST include a description of
  changes and link to the relevant spec if one exists.
- **Code review**: At least one approving review is REQUIRED
  before merge. Reviewers MUST verify constitution compliance
  for architectural changes.
- **CI/CD gates**: GitHub Actions CI MUST pass: `dotnet build`,
  `dotnet test`, SonarQube analysis, and frontend lint/test.
  Coverage regressions block merge.
- **Aspire validation**: Changes to `AppHost.cs` or service
  registration MUST be validated by running `aspire run` and
  inspecting the dashboard before PR submission.
- **Documentation**: New services, endpoints, and events MUST
  be documented in EventCatalog. OpenAPI specs MUST be kept in
  sync with implementations.
- **Incremental delivery**: Changes MUST be small, focused, and
  independently deployable. Avoid large PRs that span multiple
  bounded contexts.

## Governance

This constitution is the authoritative source of engineering
standards for the BookWorm project. It supersedes conflicting
guidance in other documents.

- **Compliance**: All PRs and code reviews MUST verify
  adherence to the principles defined herein. Violations MUST
  be resolved before merge unless an explicit, time-bound
  exception is documented.
- **Amendments**: Changes to this constitution require:
  (1) a written proposal describing the change and rationale,
  (2) review and approval, and (3) a migration plan for any
  existing code that would violate the amended principle.
- **Versioning**: This constitution follows semantic versioning.
  MAJOR: principle removal or incompatible redefinition.
  MINOR: new principle or material expansion of guidance.
  PATCH: clarifications, wording fixes, non-semantic
  refinements.
- **Review cadence**: The constitution MUST be reviewed
  quarterly or when a new service is added, whichever comes
  first.
- **Runtime guidance**: Refer to `.github/copilot-instructions.md`
  and `.github/instructions/` for day-to-day development
  conventions that complement these principles.

**Version**: 1.0.0 | **Ratified**: 2026-03-05 | **Last Amended**: 2026-03-05
