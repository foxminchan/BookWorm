# BookWorm Constitution

## Core Principles

### I. Domain-Driven Design

Every service MUST respect bounded-context boundaries. Business
logic MUST reside in the domain layer as rich aggregate models.
Domain events MUST capture state changes that are significant
to the business. Ubiquitous language from the domain MUST be
used consistently across code, documentation, and communication.

- Aggregates enforce transactional consistency boundaries.
- Value objects MUST be used for concepts without identity.
- Domain services handle logic that spans multiple aggregates.
- Cross-context communication MUST use integration events,
  never direct database access between services.

### II. Clean Architecture with Vertical Slices

Each service MUST maintain clear layer separation: Domain,
Application, and Infrastructure. Features MUST be organized
as vertical slices in `Features/{FeatureName}/` folders, each
containing a command/query, handler, and endpoint.

- The domain layer MUST have zero infrastructure dependencies.
- Application layer orchestrates via CQRS using
  `Mediator.SourceGenerator` (`ICommand<T>` / `IQuery<T>`).
- Endpoints MUST implement `IEndpoint<TResult, TRequest>` from
  BookWorm.Chassis and delegate to `ISender`.
- All endpoint, handler, and DbContext classes MUST be `sealed`.

### III. Event-Driven Integration

Inter-service communication MUST use asynchronous messaging via
Wolverine with RabbitMQ. Transactional consistency MUST be
maintained through Wolverine's durable messaging with
PostgreSQL persistence.

- Direct synchronous calls between services are prohibited
  for state-changing operations.
- Wolverine message persistence MUST be configured via
  `PersistMessagesWithPostgresql()` in each service.
- Saga orchestration MUST use Wolverine `Saga` base class
  for multi-step business processes that span service
  boundaries.

### IV. Security by Default

Secrets and API keys MUST never appear in source code or
configuration files committed to version control. User Secrets
or environment variables MUST be used for local development.

- Input validation MUST occur at service boundaries.
- PII MUST be scrubbed from all log output.
- Endpoints handling sensitive operations MUST use
  `.RequireAuthorization()` with Keycloak token introspection.
- OWASP Top 10 vulnerabilities MUST be prevented: injection,
  broken access control, cryptographic failures, SSRF, and
  insecure deserialization.

### V. Build Strictness and Type Safety

The build MUST treat all warnings as errors
(`TreatWarningsAsErrors=true`). Nullable reference types MUST
be enabled. Package versions MUST be centralized in
`Directory.Packages.props` — individual `.csproj` files MUST
NOT specify version numbers.

- Use `is null` / `is not null` instead of `== null` / `!= null`.
- Use `ArgumentNullException.ThrowIfNull()` for null guards.
- `global.json` and `NuGet.config` MUST NOT be modified unless
  explicitly requested.
- Use C# 14 features: file-scoped namespaces, primary
  constructors, pattern matching, switch expressions.

### VI. Multi-Tier Testing

Every service MUST have unit tests for domain logic and
contract tests for API surface. Integration tests MUST cover
aggregate persistence and cross-service event flows.

- Test naming: `GivenCondition_WhenAction_ThenExpectedResult()`.
- Test classes MUST be `sealed` and use Arrange-Act-Assert.
- Test projects follow `BookWorm.{Service}.{UnitTests|
ContractTests|IntegrationTests}` naming.
- Use TUnit as the test framework, Moq for mocking, Bogus for
  test data generation, and Shouldly for assertions.
- Target minimum 85% coverage for domain and application layers.

### VII. Simplicity and YAGNI

Every feature MUST use the minimum complexity needed to satisfy
requirements. Speculative abstractions, premature optimization,
and features not directly requested are prohibited.

- Do not add helpers, utilities, or abstractions for one-time
  operations.
- Do not add error handling for scenarios that cannot occur.
- Do not refactor surrounding code when fixing a bug.
- Complexity beyond the established patterns MUST be justified
  in the plan's Complexity Tracking section.

## Technology Constraints

The following technology choices are non-negotiable for all
services unless an explicit exception is approved and documented.

- **Runtime**: .NET 10 with C# 14 (`LangVersion=preview`).
- **API style**: ASP.NET Core Minimal APIs with API versioning
  (`Asp.Versioning`, `ApiVersions.V1`).
- **ORM**: EF Core with PostgreSQL, snake_case naming convention
  (`UseSnakeCaseNamingConvention()`), UUID v7 primary keys.
- **CQRS**: `Mediator.SourceGenerator` — NOT `MediatR`.
- **Messaging**: Wolverine with RabbitMQ transport.
- **Frontend**: TypeScript 5.7+, Next.js 16, React 19, pnpm
  with Turborepo monorepo under `src/Clients/`.
- **Auth**: Keycloak with token introspection.
- **Orchestration**: .NET Aspire for local development and
  service discovery.
- **Caching**: HybridCache (distributed + local tiers).

Each service owns its own database. Direct cross-service
database access is prohibited.

## Development Workflow

### Code Quality Gates

- All public APIs MUST have XML doc comments.
- All code MUST compile with zero warnings (warnings are errors).
- Code MUST conform to `.editorconfig` formatting rules.
- `just format` MUST pass before committing.

### Feature Development

1. Create a feature specification (`spec.md`) via the speckit
   workflow.
2. Plan the implementation (`plan.md`) — pass Constitution Check.
3. Generate tasks (`tasks.md`) organized by user story.
4. Implement features as vertical slices within the service's
   `Features/` directory.
5. Validate with `just build` and `just test` before committing.
6. Run Aspire (`just run`) for integration validation when
   changes affect service interactions.

### Incremental Validation

Changes MUST be validated incrementally. For backend changes,
run `dotnet build` and `dotnet test` after each logical unit.
For AppHost changes, restart Aspire and verify resource health
via the dashboard. For frontend changes, run `pnpm run dev`.

## Governance

This constitution supersedes all ad-hoc practices. All code
reviews and pull requests MUST verify compliance with these
principles. Deviations MUST be documented and justified in the
plan's Complexity Tracking section before implementation.

### Amendment Procedure

1. Propose the amendment with rationale in a pull request.
2. Document the change, its impact, and any migration steps.
3. Update the version number per semantic versioning:
   - **MAJOR**: Principle removal or backward-incompatible
     redefinition.
   - **MINOR**: New principle or materially expanded guidance.
   - **PATCH**: Clarifications, wording, or non-semantic fixes.
4. Update `LAST_AMENDED_DATE` to the date of the change.

### Compliance Review

Every plan's Constitution Check section MUST verify alignment
with all seven core principles before implementation begins.
Architecture tests in `tests/BookWorm.ArchTests/` enforce
structural rules automatically.

**Version**: 1.0.0 | **Ratified**: 2026-03-11 | **Last Amended**: 2026-03-11
