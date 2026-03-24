---
name: csharp-docs
description: "Generates XML documentation comments for C# classes, methods, properties, and constructors following .NET documentation standards. Use when adding XML docs to public APIs, documenting method parameters and return values, or writing summary tags for C# types."
---

# C# Documentation Best Practices

## General Rules

- Document all public members with XML comments; document internal members if complex
- Start `<summary>` with a present-tense, third-person verb
- Use `<remarks>` for implementation details and usage notes
- Use `<see langword>` for keywords (`null`, `true`, `false`)
- Use `<c>` for inline code, `<see cref>` for type/member references
- Use `<inheritdoc/>` to inherit from base classes or interfaces

## Example: Complete XML Documentation

```csharp
/// <summary>
/// Retrieves a book by its unique identifier.
/// </summary>
/// <param name="bookId">The unique identifier of the book to retrieve.</param>
/// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
/// <returns>A task that represents the asynchronous operation, containing the matching book.</returns>
/// <exception cref="BookNotFoundException">
/// No book with the specified <paramref name="bookId"/> exists.
/// </exception>
/// <example>
/// <code language="csharp">
/// var book = await repository.GetBookByIdAsync(42, cancellationToken);
/// Console.WriteLine(book.Title);
/// </code>
/// </example>
public async Task<Book> GetBookByIdAsync(int bookId, CancellationToken cancellationToken = default)
{
    return await _dbContext.Books.FindAsync(new object[] { bookId }, cancellationToken)
        ?? throw new BookNotFoundException(bookId);
}
```

## Methods

- `<param>`: Noun phrase without data type, starting with an article
  - Boolean params: "`<see langword="true" />` to ...; otherwise, `<see langword="false" />`."
  - Out params: "When this method returns, contains .... This parameter is treated as uninitialized."
  - Flag enum: "A bitwise combination of the enumeration values that specifies..."
  - Non-flag enum: "One of the enumeration values that specifies..."
- `<returns>`: Noun phrase without data type, starting with an article
  - Boolean returns: "`<see langword="true" />` if ...; otherwise, `<see langword="false" />`."
- Use `<paramref>` and `<typeparamref>` to reference parameters in text

## Constructors

- Summary: "Initializes a new instance of the `<ClassName>` class."

## Properties

- Read-write: "Gets or sets..."
- Read-only: "Gets..."
- Boolean: "Gets [or sets] a value that indicates whether..."
- Use `<value>` for the property value description; include defaults if applicable

## Exceptions

- Use `<exception cref>` for all directly thrown exceptions
- State the condition directly (omit "Thrown if..." or "If...")
- For nested exceptions, document only those users are likely to encounter

## Validation Checkpoint

After writing documentation, verify:
1. Every public member has a `<summary>` tag
2. All method parameters have `<param>` tags
3. Return values have `<returns>` tags
4. Thrown exceptions have `<exception cref>` tags
5. Code examples use `<example>` with `<code language="csharp">`
