---
name: csharp-async
description: "Applies C# async/await best practices including proper return types, cancellation tokens, and parallel task execution. Use when writing async methods, fixing deadlocks, optimizing Task.WhenAll usage, or reviewing asynchronous C# code for correctness."
---

# C# Async Programming Best Practices

## Naming and Return Types

- Suffix all async methods with `Async`
- Return `Task<T>` for values, `Task` for void, `ValueTask<T>` for hot-path performance
- Never return `async void` except for event handlers

## Exception Handling

- Use try/catch around await expressions
- Use `ConfigureAwait(false)` in library code to prevent deadlocks
- Propagate exceptions with `Task.FromException()` in non-async Task-returning methods

## Example: Parallel Execution with Cancellation

```csharp
public async Task<OrderSummary> GetOrderSummaryAsync(
    int orderId, CancellationToken cancellationToken = default)
{
    var orderTask = _orderRepository.GetByIdAsync(orderId, cancellationToken);
    var itemsTask = _itemRepository.GetByOrderIdAsync(orderId, cancellationToken);
    var customerTask = _customerRepository.GetByOrderAsync(orderId, cancellationToken);

    await Task.WhenAll(orderTask, itemsTask, customerTask);

    return new OrderSummary(
        await orderTask,
        await itemsTask,
        await customerTask);
}
```

## Example: Async Stream Processing

```csharp
public async IAsyncEnumerable<Product> GetProductsAsync(
    [EnumeratorCancellation] CancellationToken cancellationToken = default)
{
    await foreach (var product in _dbContext.Products
        .AsAsyncEnumerable()
        .WithCancellation(cancellationToken))
    {
        yield return product;
    }
}
```

## Example: ValueTask for Cache Hits

```csharp
public ValueTask<Book> GetBookAsync(int id, CancellationToken cancellationToken = default)
{
    if (_cache.TryGetValue(id, out var book))
        return ValueTask.FromResult(book);

    return new ValueTask<Book>(LoadBookFromDbAsync(id, cancellationToken));
}
```

## Common Pitfalls

- Never use `.Wait()`, `.Result`, or `.GetAwaiter().GetResult()` in async code
- Never mix blocking and async code — it causes deadlocks
- Always pass `CancellationToken` through the call chain
- Always await Task-returning methods; do not fire-and-forget

## Performance Patterns

- Use `Task.WhenAll()` for independent parallel operations
- Use `Task.WhenAny()` for timeouts or first-completed semantics
- Elide async/await when simply returning a single task result
- Use `CancellationToken` for all long-running or I/O-bound operations

## Validation Checkpoint

After writing async code, verify:
1. All async methods return `Task`, `Task<T>`, or `ValueTask<T>` (not `void`)
2. `CancellationToken` is accepted and forwarded in I/O-bound methods
3. Independent operations use `Task.WhenAll()` instead of sequential awaits
4. Library code uses `ConfigureAwait(false)`
5. No `.Result` or `.Wait()` calls exist in the async call chain
