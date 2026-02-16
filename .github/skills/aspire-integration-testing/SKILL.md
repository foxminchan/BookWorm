---
name: aspire-integration-testing
description: Write integration tests for BookWorm microservices using .NET Aspire's testing facilities with TUnit. Covers test fixtures, distributed application setup, endpoint discovery, resource filtering, and patterns for testing services with real PostgreSQL, RabbitMQ, Redis, Qdrant, and Keycloak dependencies.
---

## When to Use This Skill

Use this skill when:

- Writing integration tests for any BookWorm microservice
- Testing ASP.NET Core services with real PostgreSQL, Redis, RabbitMQ, or Qdrant
- Verifying service-to-service communication via gRPC or HTTP
- Testing MassTransit consumers/publishers with real RabbitMQ transport
- Testing CQRS command/query handlers end-to-end through the API layer
- Verifying health check endpoints (`/health`, `/alive`) across services
- Creating new `BookWorm.{Service}.IntegrationTests` projects
- Testing Keycloak-authenticated endpoints

## Core Principles

1. **Real Dependencies** — Use actual infrastructure (PostgreSQL, Redis, RabbitMQ,
   Qdrant) via Aspire containers, not mocks. Mocks belong in UnitTests.
2. **Resource Filtering** — Use `WithIncludeResources` to only start the resources
   your test needs; never boot the full 8-service topology for a single-service test.
3. **TUnit + TUnit.Aspire** — Use `[Test]` attribute, `AspireFixture<TAppHost>` from
   `TUnit.Aspire` for fixture lifecycle, and Shouldly for assertions. This project
   does NOT use xUnit. Fixtures are shared via
   `[ClassDataSource<T>(Shared = SharedType.PerTestSession)]` with primary constructor
   injection — never use manual `[Before(Class)]`/`[After(Class)]` for fixture lifecycle.
4. **Dynamic Discovery** — Never hard-code URLs or ports. Use
   `fixture.CreateHttpClient(resourceName)` or `fixture.App.GetEndpoint(resourceName)`.
5. **Health Check Gating** — `AspireFixture<TAppHost>` automatically waits for
   resources to reach `Running` state. Override `ResourceTimeout` if containers need
   extra time for first-run image pulls.
6. **Session Lifetime Containers** — Use `WithContainersLifetime(ContainerLifetime.Session)`
   so containers survive across test classes in a single run.
7. **Auto-Wired Infrastructure** — Any project whose path contains `IntegrationTests`
   automatically receives `Aspire.Hosting.Testing`, `TUnit.Aspire`,
   `DistributedApplicationExtensions.cs`, and `PasswordGenerator.cs` via
   Directory.Build.props. No manual PackageReference needed.
8. **Single Fixture Instance** — Use `SharedType.PerTestSession` so only ONE
   distributed application is started per test run. Each test class creating its own
   fixture multiplies container startup time and causes timeouts.

## High-Level Testing Architecture

```
┌─────────────────────────────────────────┐
│  TUnit test class                       │
│  [ClassDataSource<Fixture>              │
│   (Shared = SharedType.PerTestSession)] │
│  public sealed class Tests(Fixture f)   │
└─────────────────┬───────────────────────┘
                  │ constructor injection
                  ▼
┌──────────────────────────────────────────┐
│  AspireFixture<BookWorm_AppHost>         │
│  (from TUnit.Aspire)                    │
│  - ConfigureBuilder() override          │
│  - Manages lifecycle automatically      │
└─────────────────┬────────────────────────┘
                  │ starts
                  ▼
┌──────────────────────────────────────────┐
│  DistributedApplication                  │
│  (from BookWorm.AppHost)                │
│  filtered via WithIncludeResources(...) │
└─────────────────┬────────────────────────┘
                  │ exposes
                  ▼
┌──────────────────────────────────────────┐
│  Dynamic HTTP/gRPC Endpoints             │
└─────────────────┬────────────────────────┘
                  │ consumed by
                  ▼
┌──────────────────────────────────────────┐
│  HttpClient via                          │
│  fixture.CreateHttpClient(name)          │
└──────────────────────────────────────────┘
```

## Project Setup

### Creating a New Integration Test Project

Name the project `BookWorm.{Service}.IntegrationTests` and place it alongside
the service's UnitTests and ContractTests:

```
src/Services/{Service}/
├── BookWorm.{Service}/
├── BookWorm.{Service}.UnitTests/
├── BookWorm.{Service}.ContractTests/
└── BookWorm.{Service}.IntegrationTests/    ← new
    ├── BookWorm.{Service}.IntegrationTests.csproj
    ├── Fixtures/
    │   └── {Service}AppFixture.cs
    └── Features/
        └── {Feature}/
            └── {Feature}IntegrationTests.cs
```

### Minimal .csproj

The `Directory.Build.props` convention auto-includes packages for projects whose
path contains `IntegrationTests`:

- `Aspire.Hosting.Testing` (version 13.1.1 from Directory.Packages.props)
- `TUnit.Aspire` (provides `AspireFixture<TAppHost>` base class)
- Shared `DistributedApplicationExtensions.cs` (linked as `Extensions\`)
- Shared `PasswordGenerator.cs` (linked as `Extensions\`)
- `TUnit`, `Shouldly`, `Microsoft.Testing.Extensions.*` (via `Tests` convention)

```xml
<Project Sdk="Microsoft.NET.Sdk">
  <PropertyGroup>
    <OutputType>Exe</OutputType>
    <ExcludeFromCodeCoverage>true</ExcludeFromCodeCoverage>
  </PropertyGroup>
  <ItemGroup>
    <ProjectReference Include="..\..\..\Aspire\BookWorm.AppHost\BookWorm.AppHost.csproj" />
    <ProjectReference Include="..\BookWorm.{Service}\BookWorm.{Service}.csproj" />
  </ItemGroup>
  <!-- Optional: add WireMock.Net for mocking external APIs (OpenAI, gRPC, etc.) -->
  <ItemGroup>
    <PackageReference Include="WireMock.Net.Aspire" />
  </ItemGroup>
  <ItemGroup>
    <Using Include="Shouldly" />
  </ItemGroup>
</Project>
```

> **IMPORTANT**: The AppHost project reference is required. It provides the
> `Projects.BookWorm_AppHost` type used by `DistributedApplicationTestingBuilder`.

### Adding to the Solution

```shell
dotnet slnx add src/Services/{Service}/BookWorm.{Service}.IntegrationTests
```

## CRITICAL: File Watcher Fix for Integration Tests

When running integration tests that start `IHost` instances, the default .NET
host builder enables file watchers for configuration reload. This exhausts file
descriptor limits on Linux (CI environments).

Add this to each integration test project:

```csharp
using System.Runtime.CompilerServices;

namespace BookWorm.{Service}.IntegrationTests;

internal static class TestEnvironmentInitializer
{
    [ModuleInitializer]
    internal static void Initialize()
    {
        Environment.SetEnvironmentVariable(
            "DOTNET_HOSTBUILDER__RELOADCONFIGONCHANGE",
            "false"
        );
    }
}
```

`[ModuleInitializer]` runs before any test code executes, setting the environment
variable globally for all `IHost` instances created during the test run.

## Resource Names Reference

Use the constants from `BookWorm.Constants.Aspire` to reference resources.
Never hard-code resource name strings.

### Infrastructure Resources (`Components`)

| Constant                      | Value         | Type                             |
| ----------------------------- | ------------- | -------------------------------- |
| `Components.Postgres`         | `"postgres"`  | Azure PostgreSQL Flexible Server |
| `Components.Redis`            | `"redis"`     | Azure Managed Redis              |
| `Components.Queue`            | `"queue"`     | RabbitMQ with management plugin  |
| `Components.VectorDb`         | `"vectordb"`  | Qdrant vector database           |
| `Components.KeyCloak`         | `"keycloak"`  | Keycloak identity provider       |
| `Components.OpenAI.Resource`  | `"openai"`    | Azure OpenAI                     |
| `Components.OpenAI.Chat`      | `"chat"`      | GPT-4o-mini model                |
| `Components.OpenAI.Embedding` | `"embedding"` | text-embedding-3-large model     |

### Database Resources (`Components.Database`)

| Constant                           | Value              |
| ---------------------------------- | ------------------ |
| `Components.Database.Catalog`      | `"catalogdb"`      |
| `Components.Database.Rating`       | `"ratingdb"`       |
| `Components.Database.Finance`      | `"financedb"`      |
| `Components.Database.Ordering`     | `"orderingdb"`     |
| `Components.Database.Notification` | `"notificationdb"` |

### Service Resources (`Services`)

| Constant                | Value            | Dependencies                                                       |
| ----------------------- | ---------------- | ------------------------------------------------------------------ |
| `Services.Catalog`      | `"catalog"`      | queue, catalogdb, redis, vectordb, keycloak, blob, chat, embedding |
| `Services.Basket`       | `"basket"`       | redis, queue, catalog (gRPC), keycloak                             |
| `Services.Ordering`     | `"ordering"`     | orderingdb, queue, redis, keycloak, catalog, basket                |
| `Services.Rating`       | `"rating"`       | chat, embedding, ratingdb, mcp, queue, keycloak, chatting          |
| `Services.Chatting`     | `"chatting"`     | chat, embedding, mcp, keycloak                                     |
| `Services.Finance`      | `"finance"`      | financedb, queue                                                   |
| `Services.Notification` | `"notification"` | queue, notificationdb, email provider                              |
| `Services.Scheduler`    | `"scheduler"`    | queue (explicit start)                                             |
| `Services.McpTools`     | `"mcptools"`     | catalog                                                            |

### Health Endpoints (`Http.Endpoints`)

| Constant                               | Value       |
| -------------------------------------- | ----------- |
| `Http.Endpoints.HealthEndpointPath`    | `"/health"` |
| `Http.Endpoints.AlivenessEndpointPath` | `"/alive"`  |

## Pattern 1: App Fixture with `AspireFixture<TAppHost>` and Resource Filtering

Use the `AspireFixture<TAppHost>` base class from `TUnit.Aspire` instead of
manually implementing `IAsyncLifetime`. It handles builder creation, application
startup, resource waiting, and disposal automatically. Override `ConfigureBuilder`
to apply resource filtering and test configuration.

```csharp
using Aspire.Hosting.ApplicationModel;
using Aspire.Hosting.Testing;
using BookWorm.Common;
using BookWorm.Constants.Aspire;
using Projects;
using TUnit.Aspire;

namespace BookWorm.Catalog.IntegrationTests.Fixtures;

public sealed class CatalogAppFixture : AspireFixture<BookWorm_AppHost>
{
    protected override void ConfigureBuilder(IDistributedApplicationTestingBuilder builder)
    {
        builder
            .WithRandomParameterValues()
            .WithContainersLifetime(ContainerLifetime.Session)
            .WithRandomVolumeNames()
            .WithIncludeResources(
                Services.Catalog,
                Components.Postgres,
                Components.Database.Catalog,
                Components.Queue,
                Components.Redis,
                Components.VectorDb,
                Components.Azure.Storage.Resource
            );
    }
}
```

Key benefits over manual `IAsyncLifetime`:

- **No boilerplate** — No need to manage `DistributedApplication?` field,
  `BuildAsync`, `StartAsync`, `WaitForResourcesAsync`, or `DisposeAsync`.
- **Built-in `CreateHttpClient`** — Inherited from `AspireFixture<T>`.
- **Configurable timeout** — Override `ResourceTimeout` property (default varies;
  set to 10 minutes for first-run image pulls):
  ```csharp
  protected override TimeSpan ResourceTimeout => TimeSpan.FromMinutes(10);
  ```
- **Wait behavior** — Override `WaitBehavior` to control resource readiness
  strategy (default: `ResourceWaitBehavior.AllHealthy`).

> **IMPORTANT**: Do NOT mark the fixture class as `sealed` if TUnit.Aspire
> requires inheritance. The `CatalogAppFixture` is sealed because it is the
> final leaf class extending `AspireFixture<T>`.

### Why `WithIncludeResources` Matters

The BookWorm AppHost defines 8 services, 5 databases, RabbitMQ, Redis, Qdrant,
Keycloak, OpenAI, blob storage, a Turbo monorepo, and an API gateway. Starting
everything for a single-service test wastes minutes and gigabytes of RAM.

`WithIncludeResources` prunes the app model to only the listed resources (plus
their parent resources, discovered automatically).

## Pattern 2: Using the Fixture in TUnit Tests via `ClassDataSource`

Use `[ClassDataSource<T>(Shared = SharedType.PerTestSession)]` to share a single
fixture instance across ALL test classes in the test run. The fixture is injected
via primary constructor — no static fields or manual lifecycle methods needed.

> **CRITICAL**: Always use `SharedType.PerTestSession`. Without it, each test
> class creates its own fixture, starting containers multiple times and causing
> timeouts.

```csharp
using System.Net;
using BookWorm.Catalog.IntegrationTests.Fixtures;
using BookWorm.Constants.Aspire;
using BookWorm.Constants.Core;

namespace BookWorm.Catalog.IntegrationTests.Features;

[ClassDataSource<CatalogAppFixture>(Shared = SharedType.PerTestSession)]
public sealed class HealthCheckTests(CatalogAppFixture fixture)
{
    [Test]
    [DisplayName("Health endpoint should return OK when all dependencies are healthy")]
    public async Task GivenRunningService_WhenHealthEndpointCalled_ThenReturnsOk()
    {
        // Arrange
        using var client = fixture.CreateHttpClient(Services.Catalog);

        // Act
        var response = await client.GetAsync(Http.Endpoints.HealthEndpointPath);

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.OK);
    }

    [Test]
    [DisplayName("GET /api/v1/books should return success status code")]
    public async Task GivenCatalogService_WhenListingBooks_ThenReturnsSuccess()
    {
        // Arrange
        using var client = fixture.CreateHttpClient(Services.Catalog);

        // Act
        var response = await client.GetAsync("/api/v1/books");

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.OK);
    }
}
```

### Why `SharedType.PerTestSession` is Required

| Sharing Strategy            | Behavior                                      | Container Startups |
| --------------------------- | --------------------------------------------- | ------------------ |
| No sharing (default)        | Each test class gets its own fixture instance | N (one per class)  |
| `SharedType.PerTestSession` | One fixture shared across all test classes    | 1                  |

With 6 test classes and no sharing, the test run starts 6 separate distributed
applications with 6 sets of containers — easily exceeding a 2-minute timeout.
`SharedType.PerTestSession` ensures ONE startup for the entire test run.

## Pattern 3: Fixture per Service — Resource Selection Guide

Each service needs a different subset of resources. Below are the recommended
`WithIncludeResources` sets per service:

### Catalog (heaviest — needs blob, vector DB, AI)

```csharp
builder.WithIncludeResources([
    Services.Catalog,
    Components.Postgres,
    Components.Database.Catalog,
    Components.Queue,
    Components.Redis,
    Components.VectorDb,
    Components.Azure.Storage.Resource,
    // Note: blob container is a child of storage, auto-included
]);
```

### Finance (lightest — just DB + queue)

```csharp
builder.WithIncludeResources([
    Services.Finance,
    Components.Postgres,
    Components.Database.Finance,
    Components.Queue,
]);
```

### Ordering (needs cross-service references)

```csharp
builder.WithIncludeResources([
    Services.Ordering,
    Services.Catalog,   // gRPC dependency
    Services.Basket,    // gRPC dependency
    Components.Postgres,
    Components.Database.Ordering,
    Components.Database.Catalog,
    Components.Queue,
    Components.Redis,
]);
```

### Notification (needs queue + DB + email)

```csharp
builder.WithIncludeResources([
    Services.Notification,
    Components.Postgres,
    Components.Database.Notification,
    Components.Queue,
    // MailPit is added conditionally in run mode
]);
```

## Pattern 4: Testing MassTransit Integration Events End-to-End

Unlike ContractTests (which use `MassTransitTestHarness` with mocked dependencies),
integration tests publish real messages through RabbitMQ:

```csharp
using System.Net.Http.Json;
using BookWorm.Catalog.IntegrationTests.Fixtures;
using BookWorm.Constants.Aspire;

namespace BookWorm.Catalog.IntegrationTests.Features.Books;

[ClassDataSource<CatalogAppFixture>(Shared = SharedType.PerTestSession)]
public sealed class BookLifecycleTests(CatalogAppFixture fixture)
{
    [Test]
    public async Task CreateBook_ShouldPersistAndBeRetrievable()
    {
        // Arrange
        using var client = fixture.CreateHttpClient(Services.Catalog);

        var createRequest = new
        {
            Name = "Integration Test Book",
            Description = "A book created during integration testing",
            Price = 29.99m,
            PriceSale = 24.99m,
            CategoryId = default(Guid?),     // depends on seeded data
            PublisherId = default(Guid?),
        };

        // Act
        var createResponse = await client.PostAsJsonAsync("/api/v1/books", createRequest);

        // Assert
        createResponse.IsSuccessStatusCode.ShouldBeTrue();

        var bookId = await createResponse.Content.ReadFromJsonAsync<Guid>();
        bookId.ShouldNotBe(Guid.Empty);

        // Verify retrieval
        var getResponse = await client.GetAsync($"/api/v1/books/{bookId}");
        getResponse.IsSuccessStatusCode.ShouldBeTrue();
    }
}
```

## Pattern 5: Testing gRPC Service Communication

Several BookWorm services expose gRPC endpoints (Catalog, Basket). Test these
through the HTTP client with gRPC-Web or by verifying the dependent service
can reach the gRPC service via Aspire service discovery:

```csharp
using System.Net;
using BookWorm.Constants.Aspire;
using BookWorm.Ordering.IntegrationTests.Fixtures;

namespace BookWorm.Ordering.IntegrationTests.Features;

[ClassDataSource<OrderingAppFixture>(Shared = SharedType.PerTestSession)]
public sealed class CrossServiceTests(OrderingAppFixture fixture)
{
    [Test]
    public async Task Ordering_ShouldReachCatalogViaServiceDiscovery()
    {
        // The Ordering service references Catalog via gRPC.
        // If the ordering service is healthy, it means service discovery works.
        using var client = fixture.CreateHttpClient(Services.Ordering);

        var response = await client.GetAsync("/health");
        response.StatusCode.ShouldBe(HttpStatusCode.OK);
    }
}
```

## Pattern 6: Waiting for Specific Resource States

The shared `DistributedApplicationExtensions` provides two waiting strategies:

### Wait for a Single Resource

```csharp
// Wait for a specific resource to reach Running state
await app.WaitForResource(Services.Catalog);

// Wait for a resource to reach a specific state
await app.WaitForResource(
    Components.Postgres,
    targetState: KnownResourceStates.Running
);
```

### Wait for All Resources (Recommended)

```csharp
// Waits for every resource (except IResourceWithoutLifetime) to reach
// Running or a terminal state. Logs progress for each resource.
await app.WaitForResourcesAsync(cancellationToken: cts.Token);
```

This is the recommended approach. It:

- Skips resources that have no lifecycle (e.g., connection strings, parameters)
- Throws `DistributedApplicationException` if any resource fails to start
- Logs progress as each resource reaches its target state
- Handles resources that get deleted during startup

## Pattern 7: Shared Fixture Utilities Explained

The `tests/BookWorm.Common/` directory contains shared test infrastructure
automatically linked into integration test projects:

### `DistributedApplicationExtensions.cs`

| Method                      | Purpose                                                                                                                           |
| --------------------------- | --------------------------------------------------------------------------------------------------------------------------------- |
| `WithRandomParameterValues` | Generates random values for all `ParameterResource`. Secrets get cryptographically strong passwords; non-secrets get hex strings. |
| `WithContainersLifetime`    | Overrides `ContainerLifetimeAnnotation` on all resources. Use `ContainerLifetime.Session` for integration tests.                  |
| `WithRandomVolumeNames`     | Replaces named volumes with random names to avoid conflicts between parallel test runs.                                           |
| `WaitForResource`           | Waits for a single named resource to reach a target state (default: `Running`).                                                   |
| `WaitForResourcesAsync`     | Waits for ALL resources to reach `Running` or terminal state with structured logging.                                             |
| `WithIncludeResources`      | Prunes the app model to only include named resources and their parents. Critical for performance.                                 |

### `PasswordGenerator.cs`

Cryptographically secure password generator used by `WithRandomParameterValues`.
Produces passwords with configurable lower/upper/numeric/special character requirements.

## Pattern 8: Handling Keycloak in Tests

The AppHost conditionally adds Keycloak based on execution context:

```csharp
// In AppHost.cs
IResourceBuilder<IResource> keycloak = builder.ExecutionContext.IsRunMode
    ? builder.AddLocalKeycloak(Components.KeyCloak)
    : builder.AddHostedKeycloak(Components.KeyCloak);
```

For integration tests, the `ExecutionContext.IsPublishMode` is true by default in
`DistributedApplicationTestingBuilder`, which means the hosted Keycloak variant
is used. Depending on your test needs:

**Option A: Exclude Keycloak** — If testing endpoints that don't require auth,
omit Keycloak from `WithIncludeResources`. Services that reference Keycloak may
need the resource to start, so verify first.

**Option B: Include Keycloak** — Add `Components.KeyCloak` to the resource list.
The test will need to acquire a token before calling authenticated endpoints.

```csharp
// Include Keycloak in the resource set
builder.WithIncludeResources([
    Services.Catalog,
    Components.KeyCloak,
    Components.Postgres,
    Components.Database.Catalog,
    Components.Queue,
    Components.Redis,
    Components.VectorDb,
]);
```

## Pattern 9: Testing Event-Driven Flows Across Services

To test that an event published by one service is consumed by another:

```csharp
using System.Net.Http.Json;
using BookWorm.Constants.Aspire;
using BookWorm.Rating.IntegrationTests.Fixtures;

namespace BookWorm.Rating.IntegrationTests.Features;

[ClassDataSource<RatingAppFixture>(Shared = SharedType.PerTestSession)]
public sealed class FeedbackEventFlowTests(RatingAppFixture fixture)
{
    [Test]
    public async Task CreateFeedback_ShouldPublishIntegrationEvent()
    {
        // Arrange — both Rating and Catalog services must be running
        // so the FeedbackCreatedIntegrationEvent is consumed by Catalog
        using var ratingClient = fixture.CreateHttpClient(Services.Rating);

        // Act — create feedback via the Rating API
        var feedback = new { BookId = Guid.CreateVersion7(), Rating = 5, Comment = "Great book!" };
        var response = await ratingClient.PostAsJsonAsync("/api/v1/feedbacks", feedback);

        // Assert — verify the request succeeded
        response.IsSuccessStatusCode.ShouldBeTrue();

        // Give RabbitMQ time to deliver and Catalog time to consume
        await Task.Delay(TimeSpan.FromSeconds(3));

        // Optionally verify the side effect on the Catalog service
        // (e.g., the book's average rating was updated)
    }
}
```

## Pattern 10: Mocking External APIs with WireMock.Net Aspire

Use [WireMock.Net.Aspire](https://www.nuget.org/packages/WireMock.Net.Aspire)
to replace external API dependencies with controllable mock servers in
integration tests. This is especially useful when:

- **OpenAI** endpoints require API keys you don't have in CI
- **Cross-service gRPC** dependencies (Catalog, Basket) are too heavy to start
- **Third-party APIs** or external services should be isolated in tests

WireMock runs as a container managed by Aspire with full service discovery
support, so the service under test connects to it via the same resource name.

### Adding WireMock to Directory.Packages.props

```xml
<PackageReference Include="WireMock.Net.Aspire" Version="1.25.0" />
```

### Mocking OpenAI in a Fixture

OpenAI models (`chat`, `embedding`) require API keys. Instead of providing
real keys, replace the OpenAI-dependent service's HTTP calls with a WireMock
stub:

```csharp
using Aspire.Hosting.ApplicationModel;
using Aspire.Hosting.Testing;
using BookWorm.Common;
using BookWorm.Constants.Aspire;
using Projects;
using TUnit.Aspire;
using WireMock.Net.Aspire;

namespace BookWorm.Catalog.IntegrationTests.Fixtures;

public sealed class CatalogWithMockedAiFixture : AspireFixture<BookWorm_AppHost>
{
    protected override void ConfigureBuilder(IDistributedApplicationTestingBuilder builder)
    {
        // Filter to Catalog + infra, but EXCLUDE real OpenAI
        builder.WithIncludeResources(
            Services.Catalog,
            Components.Postgres,
            Components.Database.Catalog,
            Components.Queue,
            Components.Redis,
            Components.VectorDb
        );

        // Add a WireMock server pretending to be the OpenAI endpoint
        var mockOpenAi = builder
            .AddWireMock("mock-openai")
            .WithApiMappingBuilder(api =>
            {
                // Stub the chat completions endpoint
                api.Given(b => b
                    .WithRequest(request => request
                        .UsingPost()
                        .WithPath("/v1/chat/completions")
                    )
                    .WithResponse(response => response
                        .WithStatusCode(200)
                        .WithHeaders(h =>
                            h.Add("Content-Type", "application/json"))
                        .WithBody(@"{
                            ""id"": ""chatcmpl-test"",
                            ""object"": ""chat.completion"",
                            ""choices"": [{
                                ""index"": 0,
                                ""message"": {
                                    ""role"": ""assistant"",
                                    ""content"": ""Mocked response""
                                }
                            }]
                        }")
                    )
                );

                // Stub the embeddings endpoint
                api.Given(b => b
                    .WithRequest(request => request
                        .UsingPost()
                        .WithPath("/v1/embeddings")
                    )
                    .WithResponse(response => response
                        .WithStatusCode(200)
                        .WithHeaders(h =>
                            h.Add("Content-Type", "application/json"))
                        .WithBody(@"{
                            ""object"": ""list"",
                            ""data"": [{
                                ""object"": ""embedding"",
                                ""index"": 0,
                                ""embedding"": [0.1, 0.2, 0.3]
                            }]
                        }")
                    )
                );

                return Task.CompletedTask;
            });

        builder
            .WithRandomParameterValues()
            .WithRandomVolumeNames()
            .WithContainersLifetime(ContainerLifetime.Session);
    }

    protected override TimeSpan ResourceTimeout => TimeSpan.FromMinutes(10);
}
```

### Mocking a Cross-Service gRPC Dependency

When testing the Ordering service, it depends on Catalog and Basket via gRPC.
Instead of starting those full services, replace them with WireMock:

```csharp
var builder = await DistributedApplicationTestingBuilder
    .CreateAsync<BookWorm_AppHost>();

builder.WithIncludeResources([
    Services.Ordering,
    Components.Postgres,
    Components.Database.Ordering,
    Components.Queue,
    Components.Redis,
]);

// Replace catalog and basket with WireMock stubs
var mockCatalog = builder
    .AddWireMock(Services.Catalog)  // same resource name = transparent replacement
    .WithApiMappingBuilder(api =>
    {
        // Stub gRPC health check or specific endpoints
        api.Given(b => b
            .WithRequest(r => r.UsingGet().WithPath("/health"))
            .WithResponse(r => r.WithStatusCode(200).WithBody("Healthy"))
        );

        // Stub the book lookup endpoint used by Ordering
        api.Given(b => b
            .WithRequest(r => r
                .UsingGet()
                .WithPath("/api/v1/books/*")
            )
            .WithResponse(r => r
                .WithStatusCode(200)
                .WithHeaders(h => h.Add("Content-Type", "application/json"))
                .WithBody(@"{
                    ""id"": ""00000000-0000-0000-0000-000000000001"",
                    ""name"": ""Test Book"",
                    ""price"": 19.99
                }")
            )
        );

        return Task.CompletedTask;
    });

var mockBasket = builder
    .AddWireMock(Services.Basket)
    .WithApiMappingBuilder(api =>
    {
        api.Given(b => b
            .WithRequest(r => r.UsingGet().WithPath("/health"))
            .WithResponse(r => r.WithStatusCode(200).WithBody("Healthy"))
        );

        return Task.CompletedTask;
    });
```

> **Key insight**: When you call `AddWireMock(Services.Catalog)` using the same
> resource name as the real service, Aspire's service discovery transparently
> routes the dependent service to the WireMock container instead.

### Using Static JSON Mapping Files

For complex API mocks, use static mapping files instead of inline C# builders:

```
BookWorm.{Service}.IntegrationTests/
├── __admin/
│   └── mappings/
│       ├── openai-chat-completions.json
│       ├── openai-embeddings.json
│       └── catalog-books.json
└── Fixtures/
    └── {Service}AppFixture.cs
```

```csharp
var mappingsPath = Path.Combine(
    Directory.GetCurrentDirectory(), "__admin", "mappings");

var mockApi = builder
    .AddWireMock("mock-api")
    .WithMappingsPath(mappingsPath)
    .WithReadStaticMappings()
    .WithWatchStaticMappings();  // hot-reload during development
```

Example mapping file (`openai-chat-completions.json`):

```json
{
  "Request": {
    "Path": {
      "Matchers": [
        { "Name": "WildcardMatcher", "Pattern": "/v1/chat/completions" }
      ]
    },
    "Methods": ["POST"]
  },
  "Response": {
    "StatusCode": 200,
    "Headers": { "Content-Type": "application/json" },
    "Body": "{\"id\":\"chatcmpl-mock\",\"choices\":[{\"message\":{\"role\":\"assistant\",\"content\":\"Mocked\"}}]}"
  }
}
```

> **IMPORTANT**: Add static mapping files to the `.csproj` with
> `<CopyToOutputDirectory>PreserveNewest</CopyToOutputDirectory>`.

### When to Use WireMock vs Real Dependencies

| Scenario                                  | Approach                                                           |
| ----------------------------------------- | ------------------------------------------------------------------ |
| Database (PostgreSQL)                     | Always real — Aspire container                                     |
| Message broker (RabbitMQ)                 | Always real — Aspire container                                     |
| Cache (Redis)                             | Always real — Aspire container                                     |
| Vector DB (Qdrant)                        | Always real — Aspire container                                     |
| OpenAI / LLM APIs                         | **WireMock** — requires API keys, non-deterministic                |
| Cross-service HTTP/gRPC (Catalog, Basket) | Real if testing integration; **WireMock** if isolating one service |
| Keycloak auth                             | Real for auth flow tests; omit or WireMock for non-auth tests      |
| Email provider (MailPit)                  | Run-mode only; **WireMock** in tests if needed                     |

## Common Patterns Summary

| Pattern                           | Use Case                                                    |
| --------------------------------- | ----------------------------------------------------------- |
| AspireFixture (Pattern 1)         | Single-service fixture with `AspireFixture<T>` base class   |
| ClassDataSource (Pattern 2)       | Sharing fixture via `SharedType.PerTestSession` + injection |
| Resource Selection (Pattern 3)    | Per-service resource requirements guide                     |
| MassTransit E2E (Pattern 4)       | Real message broker integration events                      |
| gRPC Communication (Pattern 5)    | Cross-service gRPC via service discovery                    |
| Resource Waiting (Pattern 6)      | Ensuring resources are ready before tests                   |
| Shared Utilities (Pattern 7)      | Using the pre-built test extensions                         |
| Keycloak Auth (Pattern 8)         | Handling authentication in tests                            |
| Event-Driven Flows (Pattern 9)    | Cross-service event propagation                             |
| WireMock API Mocking (Pattern 10) | Replace external/cross-service APIs with stubs              |

## Differences from UnitTests and ContractTests

| Aspect           | UnitTests                      | ContractTests                        | IntegrationTests                      |
| ---------------- | ------------------------------ | ------------------------------------ | ------------------------------------- |
| **Framework**    | TUnit + Moq + Shouldly         | TUnit + MassTransit Harness + Verify | TUnit + TUnit.Aspire + Shouldly       |
| **Dependencies** | All mocked                     | MassTransit in-memory harness        | Real containers via Aspire            |
| **Database**     | None                           | None                                 | Real PostgreSQL                       |
| **Messaging**    | None                           | In-memory bus                        | Real RabbitMQ                         |
| **Scope**        | Single handler/validator       | Consumer/publisher contracts         | Full HTTP/gRPC request flow           |
| **Speed**        | Milliseconds                   | Seconds                              | Minutes (first run, cached after)     |
| **Convention**   | `BookWorm.{Service}.UnitTests` | `BookWorm.{Service}.ContractTests`   | `BookWorm.{Service}.IntegrationTests` |

## Tricky / Non-Obvious Tips

| Problem                                | Solution                                                                                                                           |
| -------------------------------------- | ---------------------------------------------------------------------------------------------------------------------------------- |
| Tests timeout on startup               | Override `ResourceTimeout` in fixture to 10+ minutes. Container images must be pulled on first run.                                |
| Port conflicts between test runs       | `WithRandomParameterValues()` and `WithRandomVolumeNames()` handle this. Never hard-code ports.                                    |
| Too many containers starting           | Use `WithIncludeResources` to filter. The full AppHost starts 15+ containers.                                                      |
| Multiple fixture instances             | Always use `SharedType.PerTestSession` with `[ClassDataSource]`. Without it, each test class starts its own containers.            |
| Keycloak variant differs in tests      | `DistributedApplicationTestingBuilder` uses publish mode by default, so `AddHostedKeycloak` is used instead of `AddLocalKeycloak`. |
| Run-mode-only resources skipped        | Scalar, MCP Inspector, and K6 are wrapped in `if (builder.ExecutionContext.IsRunMode)` — they won't start in tests.                |
| Scheduler has explicit start           | `BookWorm.Scheduler` uses `.WithExplicitStart()`. It won't auto-start in tests; trigger it manually if needed.                     |
| Container lifetime and caching         | Use `WithContainersLifetime(ContainerLifetime.Session)` so containers are reused across test classes in the same test run.         |
| File descriptor exhaustion on Linux CI | Add the `TestEnvironmentInitializer` with `[ModuleInitializer]` to disable config file watching.                                   |
| OpenAI resources in tests              | OpenAI models require API keys. Either exclude them from `WithIncludeResources`, provide test keys, or use WireMock to stub.       |
| Flaky tests due to timing              | Use `WaitForResourcesAsync` instead of `Task.Delay`. For async event processing, poll for expected state with a timeout.           |
| WireMock resource name collision       | Use the same resource name as the real service (e.g., `Services.Catalog`) to transparently redirect via service discovery.         |
| WireMock mappings not found            | Ensure mapping JSON files have `<CopyToOutputDirectory>PreserveNewest</CopyToOutputDirectory>` in the csproj.                      |
| Non-deterministic AI responses         | Always WireMock OpenAI endpoints in tests — real LLM responses vary between runs and break assertions.                             |

## CI/CD Integration

### GitHub Actions Example

```yaml
name: Integration Tests

on:
  push:
    branches: [main]
  pull_request:
    branches: [main]

jobs:
  integration-tests:
    runs-on: ubuntu-latest
    timeout-minutes: 30

    steps:
      - uses: actions/checkout@v4

      - name: Setup .NET
        uses: actions/setup-dotnet@v4
        with:
          dotnet-version: 10.0.x

      - name: Restore
        run: dotnet restore

      - name: Build
        run: dotnet build --no-restore -c Release

      - name: Run Integration Tests
        run: |
          dotnet test --no-build -c Release \
            --filter "FullyQualifiedName~IntegrationTests" \
            --logger trx \
            --collect:"XPlat Code Coverage"
        env:
          DOTNET_HOSTBUILDER__RELOADCONFIGONCHANGE: "false"

      - name: Upload test results
        uses: actions/upload-artifact@v4
        if: always()
        with:
          name: integration-test-results
          path: "**/TestResults/*.trx"
```

### Key CI Considerations

- Docker must be available (ubuntu-latest includes Docker)
- First run pulls container images — allow 10+ minutes
- Subsequent runs with `ContainerLifetime.Session` reuse containers within a job
- Set `DOTNET_HOSTBUILDER__RELOADCONFIGONCHANGE=false` as environment variable

## Best Practices

1. **One fixture per service** — Each service gets its own fixture extending
   `AspireFixture<BookWorm_AppHost>` with its minimal resource set. Share the
   fixture across test classes via `[ClassDataSource<T>(Shared = SharedType.PerTestSession)]`.
2. **Use `ClassDataSource` with `PerTestSession`** — This ensures a single fixture
   instance and ONE container startup per test run. Never use `[Before(Class)]`/
   `[After(Class)]` for fixture lifecycle — that creates separate instances per class.
3. **Primary constructor injection** — Inject the fixture via primary constructor
   parameters: `public sealed class MyTests(MyFixture fixture)`. Access it as
   `fixture` throughout the test methods. No static fields needed.
4. **Use constants** — Always reference `Services.*` and `Components.*` from
   `BookWorm.Constants.Aspire`. Never use raw strings.
5. **Filter aggressively** — `WithIncludeResources` is the most impactful
   optimization. A Catalog-only test should not start the Ordering database.
6. **Randomize everything** — Call `WithRandomParameterValues()` and
   `WithRandomVolumeNames()` to avoid conflicts in parallel CI runs.
7. **Session-scoped containers** — Use `ContainerLifetime.Session` to amortize
   container startup across all test classes.
8. **Override `ResourceTimeout`** — Set `protected override TimeSpan ResourceTimeout`
   to 10+ minutes for first-run image pulls. Default may be too short.
9. **Follow naming conventions** — Project name must contain `IntegrationTests`
   to trigger the Directory.Build.props auto-wiring.
10. **Keep UnitTests fast** — Integration tests supplement, not replace, UnitTests.
    Business logic validation stays in UnitTests with mocks.
11. **Test the boundaries** — Integration tests verify HTTP endpoints, database
    persistence, message broker delivery, and service discovery — the things
    UnitTests cannot cover.
12. **Let `AspireFixture<T>` manage lifecycle** — Do not manually call
    `BuildAsync`, `StartAsync`, `WaitForResourcesAsync`, or `DisposeAsync`.
    The base class handles all of this. Only override `ConfigureBuilder` and
    optionally `ResourceTimeout` / `WaitBehavior`.
13. **Use WireMock for non-deterministic APIs** — OpenAI, external payment
    gateways, and third-party services should be replaced with WireMock stubs
    to ensure deterministic, repeatable test results without API keys.
14. **Prefer transparent replacement** — When mocking a cross-service dependency,
    use the same resource name (`Services.Catalog`) with `AddWireMock` so the
    service under test connects via standard service discovery without config changes.
