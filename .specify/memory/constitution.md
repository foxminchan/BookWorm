# BookWorm Constitution

## Core Principles

### I. Domain-Driven Design (NON-NEGOTIABLE)

Every service follows Domain-Driven Design principles with clear domain boundaries. Services must maintain their own domain models, use ubiquitous language, and implement proper aggregates. Domain logic must be isolated from infrastructure concerns and remain testable independently. Cross-service communication occurs only through well-defined integration events using the `BookWorm.Contracts` namespace.

### II. Microservices Architecture & Service Independence

Each service is independently deployable and owns its data. Services communicate through gRPC for synchronous operations and integration events for asynchronous communication. No direct database access between services. Service boundaries align with domain boundaries and business capabilities. Frontend applications (Blazor Admin and Next.js Storefront) consume backend services through well-defined API gateways and maintain clear separation of concerns.

### III. Test-First Development (NON-NEGOTIABLE)

100% business logic coverage is mandatory. Tests written using `Given_When_Then` pattern before implementation. All domain and application logic must have comprehensive unit tests. Mock external dependencies including repositories, services, and infrastructure components. Tests must be fast, independent, and deterministic.

### IV. Clean Architecture & Vertical Slice

Services follow Clean Architecture with clear separation between Domain, Application, Infrastructure, and API layers. Implement Vertical Slice Architecture for feature organization. Use CQRS with MediatR for command/query separation. Domain events handle side effects and cross-cutting concerns.

### V. .NET 10 & Modern C# Standards

Use latest C# 14 features exclusively. Follow `.editorconfig` formatting rules. Use file-scoped namespaces, primary constructors for immutable properties, expression-bodied members, pattern matching, and `is null`/`is not null` checks. Trust null annotations and avoid unnecessary null checks.

## Technology & Architecture Standards

### Cloud-Native with Aspire

All services run in Aspire orchestration environment. Support containerization with Docker. Implement proper health checks and observability. Use HybridCache for performance optimization. Support horizontal scaling and cloud deployment patterns.

### Frontend Architecture & Technology Stack

**Admin Backoffice**: Built with Blazor Server/WebAssembly for administrative operations. Implements role-based access control, comprehensive data management interfaces, and real-time dashboards. Must follow component-based architecture with proper state management and validation.

**Storefront Website**: Built with Next.js for optimal user experience and SEO. Implements server-side rendering, static generation where appropriate, and progressive web app capabilities. Must maintain responsive design, accessibility standards, and performance optimization.

### Event-Driven Architecture

Implement outbox/inbox patterns for reliable event processing. Use saga patterns for orchestration and choreography. Support event sourcing for domain events. All integration events must follow naming convention `[Action][Entity]IntegrationEvent` and include minimal necessary data.

### AI & Modern Tooling Integration

Integrate AI components using Agent Framework for multi-agent workflows. Support Model Context Protocol (MCP) for standardized AI tooling. Enable agent-to-agent communication via A2A Protocol. Use text embedding and chatbot functionality where applicable. Frontend applications must integrate AI features seamlessly through dedicated AI service endpoints.

### API Gateway & Frontend Integration

Implement API Gateway pattern for frontend-to-backend communication. Support authentication/authorization flow between frontend applications and backend services. Maintain consistent API versioning across all client applications. Enable real-time communication through SignalR for admin dashboards and user notifications.

## Development Workflow & Quality Gates

### Code Quality & Standards

Follow DDD principles strictly. Use explicit type declarations when type isn't obvious. Implement proper error handling with domain exceptions. Use Repository pattern for data access, Specification pattern for complex queries, and Factory/Builder patterns for object creation.

**Frontend Code Standards**: Blazor components must follow component lifecycle best practices, proper dependency injection, and state management patterns. Next.js application must use TypeScript, follow React best practices, implement proper error boundaries, and maintain consistent folder structure. Both frontend applications must include comprehensive unit and integration tests.

### Pull Request Requirements

All PRs require maintainer approval and passing CI/CD checks including SonarQube analysis and Snyk security scans. Include comprehensive unit tests for new features. Update documentation for functionality changes. Link related issues using keywords (Fixes #123). Address reviewer feedback promptly.

### Documentation & API Standards

Use OpenAPI for REST APIs and AsyncAPI for event-driven endpoints. Maintain EventCatalog for centralized architecture documentation. Include code comments for complex logic. Provide clear examples for API changes. Keep GitHub Wiki updated for comprehensive project documentation.

**Frontend Documentation**: Document component APIs, state management patterns, and integration patterns. Maintain Storybook for Blazor components and component documentation for Next.js. Include deployment guides for both admin and storefront applications. Document authentication flows and API consumption patterns.

## Governance

This constitution supersedes all other development practices and guidelines. All PRs and code reviews must verify compliance with these principles. Complexity must be justified and aligned with business value. Architecture violations detected by tests must be resolved before merging.

For detailed development guidance, refer to `AGENTS.md` and `.github/CONTRIBUTING.md`. Integration event namespaces must never be modified as it disrupts the messaging system routing.

**Version**: 1.0.0 | **Ratified**: 2025-09-27 | **Last Amended**: 2025-09-27
