# BookWorm — Copilot instructions

## System overview

BookWorm is a microservices-based system built around .NET Aspire for service orchestration, local developer experience, and cloud-ready composition. It combines:

- Microservices with clear bounded contexts (e.g., Catalog, Orders, Users/Identity, Recommendations, etc.).
- AI Agents integrated as first-class components to augment features (e.g., content enrichment, personalized recommendations, summarization, routing, or workflow orchestration).
- A unified developer experience via Aspire’s AppHost, service discovery, health, and observability.
- A TypeScript-based frontend or UI shell (if present) communicating with service APIs and/or a gateway.
- Automated local and CI test flows with TUnit and Moq for .NET services.

Goals:
- Demonstrate modern .NET microservices patterns with Aspire.
- Show safe, incremental adoption of AI agents/tools within core flows.
- Provide end-to-end reference for local dev, testing, and observability.

## Tech stack and key dependencies

- Runtime: .NET (C#; use the latest language features as configured in this repo).
- Orchestration: .NET Aspire (AppHost, ServiceDefaults).
- Application code: C# (primary), TypeScript (frontend or tooling), PowerShell/Shell for scripts, Justfile for dev ergonomics (if present).
- Testing: TUnit, Moq.
- Data and messaging: Inspect code for concrete providers (e.g., PostgreSQL/MSSQL, Redis, Kafka/Azure Service Bus/RabbitMQ). Do not assume—search the code.
- Observability: Aspire-enabled OpenTelemetry wiring, health endpoints, dashboards (verify in AppHost).
- AI: Agent orchestration and tools. Verify implemented providers and key management (e.g., Azure OpenAI, OpenAI, local models, semantic memory, vector stores).

Where to confirm:
- AppHost project: references to services, dashboards, storage/messaging resources.
- Service projects: Program.cs/ServiceDefaults for DI, HTTP endpoints, and telemetry setup.
- Frontend: TypeScript project/package.json if present.
- Infrastructure scripts: PowerShell/Shell/Justfile tasks.

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
   - From the frontend folder: `bun i`, then `bun run dev`/`build`.
   - Ensure API base URLs match local dev settings or proxy via API Gateway/BFF.

4) Configuration and secrets
   - Use User Secrets or environment variables for API keys (especially AI model providers).
   - Never commit secrets. Check `README`/scripts for local `.env` conventions if provided.

5) Tests
   - Run `dotnet test` from the solution root or specific test projects.
   - Follow TUnit patterns; use Moq for mocks.

## Common tasks (suggested; verify with repo)

- just build: compile all projects
- just run: start AppHost and services
- just test: run all unit tests
- just fmt: apply formatting
- just clean: clean artifacts

If no Justfile: use the equivalent `dotnet` and package manager commands.

## Security and compliance

- Never commit secrets or API keys. Prefer User Secrets/local env for dev; key vaults for cloud.
- Validate inputs at service boundaries.
- Consider rate limiting and circuit breakers for external AI/model calls.
- Scrub PII in logs; redact prompts/responses as needed.
