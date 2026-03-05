---
description: "DDD and .NET architecture guidelines"
applyTo: "**/*.cs,**/*.csproj,**/Program.cs,**/*.razor"
name: Architecture-DDD-.NET
---

# DDD & .NET Architecture

## Core Principles

- **Ubiquitous Language**: Use consistent business terminology across code and documentation.
- **Bounded Contexts**: Clear service boundaries with well-defined responsibilities.
- **Aggregates**: Enforce consistency boundaries and transactional integrity.
- **Rich Domain Models**: Business logic belongs in the domain layer, not application services.
- **Domain Events**: Capture business-significant state changes; use for cross-context communication.

## Layer Responsibilities

- **Domain**: Aggregates, value objects, domain services, domain events, specifications.
- **Application**: Orchestrate domain operations, DTOs, input validation, dependency injection.
- **Infrastructure**: Repositories, event bus, ORM mapping, external service adapters.

## SOLID Compliance

- **SRP**: One reason to change per class.
- **OCP**: Open for extension, closed for modification.
- **LSP**: Subtypes substitutable for base types.
- **ISP**: No client forced to depend on unused methods.
- **DIP**: Depend on abstractions, not concretions.

## Testing

- Name tests: `GivenCondition_WhenAction_ThenExpectedResult()`.
- Unit tests for domain logic; integration tests for aggregate boundaries; acceptance tests for user scenarios.
- Minimum 85% coverage for domain and application layers.

## Security & Performance

- Implement authorization at aggregate level.
- Use `async`/`await` for all I/O; optimize data access and caching.
- Use `decimal` for monetary calculations; maintain proper rounding.