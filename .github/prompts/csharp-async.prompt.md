---
agent: CSharp-Expert
description: "Get best practices for C# async programming"
tools:
  - "search/changes"
  - "search/codebase"
  - "edit/editFiles"
  - "read/problems"
model: Claude Sonnet 4.5 (copilot)
---

# C# Async Programming Best Practices

Your goal is to help me follow best practices for asynchronous programming in C#.

## Naming Conventions

- Use the 'Async' suffix for all async methods
- Match method names with their synchronous counterparts when applicable (e.g., `GetDataAsync()` for `GetData()`)

## Return Types

- Return `Task<T>` when the method returns a value
- Return `Task` when the method doesn't return a value
- Use `ValueTask<T>` **only when measured to improve performance** (not as default)
- Avoid returning `void` for async methods except for event handlers
- For Mediator handlers, prefer `ValueTask<Unit>` for commands with no return value

## Exception Handling

- Use try/catch blocks around await expressions
- Avoid swallowing exceptions in async methods
- Use `ConfigureAwait(false)` when appropriate to prevent deadlocks in library code
- Propagate exceptions with `Task.FromException()` instead of throwing in async Task returning methods

## Cancellation Token Usage

- **Always accept `CancellationToken`** as a parameter in async methods (prefer as last parameter)
- **Pass cancellation tokens through** the entire async call chain
- Call `ThrowIfCancellationRequested()` in loops and before long-running operations
- Make delays cancelable: `Task.Delay(milliseconds, cancellationToken)`
- **Propagate tokens to all async operations**: database calls, HTTP requests, message publishing
- Use linked `CancellationTokenSource` for composite cancellations: `CancellationTokenSource.CreateLinkedTokenSource(token1, token2)`
- Implement timeout with cancellation: `using var cts = new CancellationTokenSource(timeout); cts.Token`
- Handle `OperationCanceledException` gracefully in application code

## Performance

- Use `Task.WhenAll()` for parallel execution of multiple tasks
- Use `Task.WhenAny()` for implementing timeouts or taking the first completed task
- **Always cancel pending tasks** when using `Task.WhenAny()` to prevent resource leaks
- Avoid unnecessary async/await when simply passing through task results
- Stream large JSON responses: use `HttpCompletionOption.ResponseHeadersRead` with `ReadAsStreamAsync()`

## Common Pitfalls

- Never use `.Wait()`, `.Result`, or `.GetAwaiter().GetResult()` in async code
- Avoid mixing blocking and async code
- Don't create async void methods (except for event handlers)
- Always await Task-returning methods

## Implementation Patterns

- Implement the async command pattern for long-running operations
- Use async streams (`IAsyncEnumerable<T>`) for processing sequences asynchronously
- Follow the task-based asynchronous pattern (TAP) for public APIs
- Implement `IAsyncDisposable` for resources requiring async cleanup; use `await using`
- For MassTransit consumers, always pass `context.CancellationToken` to async operations
- For gRPC services, use `ServerCallContext.CancellationToken`
- For ASP.NET endpoints, accept `CancellationToken` as parameter from the framework

When reviewing my C# code, identify these issues and suggest improvements that follow these best practices.
