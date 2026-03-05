---
description: "Guidelines for building C# applications"
applyTo: "**/*.cs"
name: CSharp
---

# C# Development

## Language & Style

- Always use C# 14 features; never change `global.json` or `NuGet.config` unless asked.
- Apply `.editorconfig` formatting rules.
- Prefer file-scoped namespace declarations and single-line using directives.
- Use `var` when the type is obvious.
- Use primary constructors for classes with immutable properties.
- Use expression-bodied members, pattern matching, and switch expressions.
- Use `nameof` instead of string literals when referring to member names.
- Insert a newline before opening curly braces of code blocks.

## Naming Conventions

- PascalCase for types, methods, and public members.
- camelCase for private fields and local variables.
- Prefix interfaces with `I` (e.g., `IUserService`).
- Comments explain **why**, not what.

## Nullable Reference Types

- Declare variables non-nullable; check for `null` at entry points.
- Use `is null` / `is not null` instead of `== null` / `!= null`.
- Trust C# null annotations — skip redundant null checks.

## Error Handling

- Use `ArgumentNullException.ThrowIfNull(x)` for null guards.
- Choose precise exception types (`ArgumentException`, `InvalidOperationException`).
- Never throw or catch base `Exception`; never swallow errors silently.

## Async

- All async methods end with `Async`.
- Always `await` — no fire-and-forget; use cancellation instead of timeouts.
- Accept and propagate `CancellationToken` end-to-end.
- Use `ConfigureAwait(false)` in library code; omit in app entry.
- Default to `Task`; use `ValueTask` only when measured to help.

## Testing

- Use TUnit for tests; Moq for mocking.
- Copy existing style in nearby files for test method naming.
- XML doc comments on all public APIs; include `<example>` and `<code>` where applicable.