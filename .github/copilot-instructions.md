---
applyTo: "**/*.cs"
---

# Instructions for GitHub Copilot

## General

- Make only high confidence suggestions when reviewing code changes.
- Always use the latest version C#, currently C# 13 features.
- Never change global.json unless explicitly asked to.
- Never change nuget.config files unless explicitly asked to.

## Formatting

- Apply code-formatting style defined in `.editorconfig`.
- Prefer file-scoped namespace declarations and single-line using directives.
- Insert a newline before the opening curly brace of any code block (e.g., after `if`, `for`, `while`, `foreach`, `using`, `try`, etc.).
- Ensure that the final return statement of a method is on its own line.
- Use pattern matching and switch expressions wherever possible.
- Use `nameof` instead of string literals when referring to member names.
- Place private class declarations at the bottom of the file.
- Use `var` when the type is obvious.
- Use `Primary Constructor` for classes with immutable properties.
- Use `Expression-bodied members` for methods and properties.

### Nullable Reference Types

- Declare variables non-nullable, and check for `null` at entry points.
- Always use `is null` or `is not null` instead of `== null` or `!= null`.
- Trust the C# null annotations and don't add null checks when the type system says a value cannot be null.
