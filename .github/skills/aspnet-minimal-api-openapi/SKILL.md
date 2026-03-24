---
name: aspnet-minimal-api-openapi
description: "Generates ASP.NET Minimal API endpoints with typed responses, OpenAPI/Swagger documentation, and proper request validation. Use when building REST APIs, adding Swagger docs, creating .NET API endpoints, or configuring OpenAPI schemas in ASP.NET."
---

# ASP.NET Minimal API with OpenAPI

## API Organization

- Group related endpoints using `MapGroup()` extension
- Use endpoint filters for cross-cutting concerns
- Structure larger APIs with separate endpoint classes
- Use a feature-based folder structure for complex APIs

## Request and Response Types

- Define explicit request and response DTOs/models using record types
- Apply `[Required]` and other validation attributes to enforce constraints
- Use the ProblemDetailsService and StatusCodePages for standard error responses

## Type Handling

- Use strongly-typed route parameters with explicit type binding
- Use `Results<T1, T2>` to represent multiple response types
- Return `TypedResults` instead of `Results` for strongly-typed responses

## OpenAPI Documentation

- Use the built-in OpenAPI document support added in .NET 9
- Add operationIds using the `WithName` extension method
- Add descriptions to properties and parameters with `[Description()]`
- Use document transformers for servers, tags, and security schemes
- Use schema transformers to customize OpenAPI schemas

## Example: Typed Endpoint with OpenAPI

```csharp
var group = app.MapGroup("/api/books")
    .WithTags("Books");

group.MapGet("/{id:int}", async (int id, BookDbContext db) =>
{
    var book = await db.Books.FindAsync(id);
    return book is not null
        ? TypedResults.Ok(book)
        : TypedResults.NotFound();
})
.WithName("GetBookById")
.WithDescription("Retrieves a book by its unique identifier")
.Produces<Book>(StatusCodes.Status200OK)
.Produces(StatusCodes.Status404NotFound);

group.MapPost("/", async ([Description("The book to create")] CreateBookRequest request, BookDbContext db) =>
{
    var book = new Book { Title = request.Title, Author = request.Author };
    db.Books.Add(book);
    await db.SaveChangesAsync();
    return TypedResults.Created($"/api/books/{book.Id}", book);
})
.WithName("CreateBook")
.Produces<Book>(StatusCodes.Status201Created);
```

## Validation Checkpoint

After generating an endpoint, verify:
1. All route parameters have explicit type constraints (e.g., `{id:int}`)
2. Response types use `TypedResults` (not `Results`)
3. `WithName()` is set for every endpoint
4. Request DTOs have validation attributes where required
5. `Produces<T>()` declarations match all possible response codes
