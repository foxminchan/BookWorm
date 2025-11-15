# Contributing to BookWorm

Thank you for your interest in contributing to BookWorm!

We appreciate your help in making BookWorm better. Please follow the guidelines in this document to ensure a smooth contribution process.

## Table of Contents

- [Contributing to BookWorm](#contributing-to-bookworm)
  - [Table of Contents](#table-of-contents)
  - [Getting Started](#getting-started)
  - [Development Workflow](#development-workflow)
  - [Agent Workflow](#agent-workflow)
    - [Available Agents](#available-agents)
    - [Agent Collaboration Workflow](#agent-collaboration-workflow)
    - [Typical Workflows](#typical-workflows)
    - [Using Agents in Your Workflow](#using-agents-in-your-workflow)
  - [Coding Standards](#coding-standards)
  - [Integration Events Standards](#integration-events-standards)
  - [Design Patterns](#design-patterns)
  - [Testing Guidelines](#testing-guidelines)
    - [Core Testing Principles](#core-testing-principles)
    - [Test Organization](#test-organization)
    - [Test Quality](#test-quality)
    - [Tools and Resources](#tools-and-resources)
  - [Pull Request Process](#pull-request-process)
    - [Pull Request Process](#pull-request-process-1)
  - [Need Help?](#need-help)


## Getting Started

1. **Fork the Repository**: Start by forking the main BookWorm repository.
2. **Clone Your Fork**: git clone `https://github.com/YOUR-USERNAME/BookWorm.git`
3. **Add Upstream Remote**: git remote add upstream `https://github.com/ORIGINAL-OWNER/BookWorm.git`
4. **Auto LFE**: Ensure your Git configuration is set to handle line endings correctly:
   ```bash
   git config --global core.autocrlf input
   ```
5. **Review Project Guide**: See [AGENTS.md](../AGENTS.md) for detailed setup instructions, prerequisites, and project architecture overview.

## Development Workflow

1. **Create a Branch**: `git checkout -b feature/your-feature-name`
2. **Make Changes**: Follow the coding conventions described below
3. **Commit Changes**: Use descriptive commit messages
4. **Push to Your Fork**: `git push origin feature/your-feature-name`
5. **Create a Pull Request**: Submit your changes for review

For the branching strategy, please refer to the [Git Flow](https://nvie.com/posts/a-successful-git-branching-model/) model.

## Agent Workflow

BookWorm includes AI agents to assist with development tasks. These agents work collaboratively through a structured handoff system:

### Available Agents

1. **CSharp-Expert** - Assists with C#/.NET development tasks
   - Provides clean, well-designed code following .NET conventions
   - Covers security, design patterns, SOLID principles
   - Helps with async programming, performance optimization

2. **Debug** - Systematic debugging and bug resolution
   - Identifies and analyzes bugs methodically
   - Implements targeted fixes with verification
   - Ensures no regressions through comprehensive testing

3. **Planner** - Creates implementation plans for features/refactoring
   - Generates detailed implementation plans
   - Breaks down complex tasks into actionable steps
   - Documents requirements and testing strategies

4. **Code-Reviewer** - Reviews code for quality and security
   - Performs systematic code quality reviews
   - Checks security vulnerabilities and best practices
   - Validates architecture and Aspire-specific patterns

### Agent Collaboration Workflow

The agents collaborate through handoffs to provide comprehensive development support:

```mermaid
graph TD
    P[Planner] -->|Implementation Plan| CE[CSharp-Expert]
    P -->|Review Plan| CR[Code-Reviewer]
    CE -->|Request Review| CR
    CE -->|Debug Issues| D[Debug]
    CE -->|Plan Complex Changes| P
    D -->|Get Expert Help| CE
    D -->|Review Fix| CR
    D -->|Plan Refactoring| P
    CR -->|Fix Issues| CE
    CR -->|Create Refactoring Plan| P

    style P fill:#e1f5ff
    style CE fill:#fff4e1
    style D fill:#ffe1e1
    style CR fill:#e1ffe1
```

### Typical Workflows

**Feature Development:**
1. Start with **Planner** to create implementation plan
2. Hand off to **CSharp-Expert** for implementation
3. Use **Code-Reviewer** to validate changes
4. Use **Debug** if issues arise

**Bug Fixing:**
1. Start with **Debug** to identify and fix bugs
2. Hand off to **CSharp-Expert** for complex solutions
3. Use **Code-Reviewer** to review the fix
4. Use **Planner** if architectural changes needed

**Code Review:**
1. Start with **Code-Reviewer** for systematic review
2. Hand off to **CSharp-Expert** to address issues
3. Use **Planner** for major refactoring recommendations

### Using Agents in Your Workflow

- Agents are available in the `.github/agents/` directory
- Each agent has specific expertise and tools
- Agents can hand off work to other agents based on task requirements
- Follow agent recommendations for maintaining code quality and consistency

> [!TIP]
> When working on complex features, start with the **Planner** agent to create a comprehensive implementation plan before coding.

## Coding Standards

- Follow DDD (Domain-Driven Design) principles
- Use the latest C# features and idioms
- Implement unit tests for all business logic
- Maintain service boundaries - avoid direct cross-service dependencies
- Follow `.editorconfig` settings for code formatting
- Prefer explicit type declarations when type isn't obvious
- Use primary constructors for classes with immutable properties
- Use expression-bodied members when appropriate

Example:

```csharp
public sealed class Book
{
   public string Title { get; private set; }
   public string Author { get; private set; }

   public Book(string title, string author)
   {
      Title = !string.IsNullOrWhiteSpace(title)
         ? title
         : throw new CatalogDomainException("Title cannot be empty.");
      Author = !string.IsNullOrWhiteSpace(author)
         ? author
         : throw new CatalogDomainException("Author cannot be empty.");
   }
}

public sealed class BookService
{
   public Book GetBook(string title, string author) => new Book(title, author);
}
```

## Integration Events Standards

When working with integration events for cross-service communication:

- Always use the `BookWorm.Contracts` namespace when declaring integration events
- Follow the naming convention `[Action][Entity]IntegrationEvent` (e.g., `BookCreatedIntegrationEvent`)
- Include only necessary data in integration events to minimize payload size
- Implement proper serialization attributes for all event properties

> [!CAUTION]
> Do not modify namespaces for `Integration Events` as it will disrupt the messaging system. The event bus relies on consistent namespace conventions for proper routing.

```csharp
namespace BookWorm.Contracts;

public sealed record UserCheckedOutIntegrationEvent(
   Guid OrderId,
   Guid BasketId,
   string? Email,
   decimal TotalMoney
) : IntegrationEvent;
```

## Design Patterns

- Use CQRS with MediatR when applicable
- Repository pattern for data access
- Domain Events for cross-service communication
- Avoid circular dependencies between services
- Keep services independently deployable
- Use Value Objects for immutable types
- Use Domain Events for side effects
- Use Domain Services for complex business logic
- Use Factories or Builders for object creation
- Use Specification pattern for complex queries

## Testing Guidelines

### Core Testing Principles

- **100% Business Logic Coverage**: Write unit tests for all domain and business logic components
- **Descriptive Test Names**: Use the `Given_When_Then` pattern for test naming (e.g., `Given_ValidBook_When_AddingToLibrary_Then_SuccessReturned`)
- **Isolation**: Mock all external dependencies including repositories, services, and infrastructure components
- **Comprehensive Scenarios**: Test both happy paths and edge cases, including validation failures and exception handling

### Test Organization

- Group tests by feature or domain entity
- Create separate test fixtures for different testing scenarios
- Use appropriate test attributes for categorization

### Test Quality

- Aim for high code coverage in domain logic (minimum 80%)
- Follow TUnit conventions for test methods and classes
- Avoid testing implementation details; focus on behaviors
- Write deterministic tests that don't depend on environment or timing
- Keep tests fast, independent, and repeatable
- Use descriptive test names that clearly indicate what is being tested
- Ensure tests are isolated and don't share state

### Tools and Resources

- **Testing Framework**: Use **TUnit** (not xUnit/NUnit/MSTest)
- **Mocking**: Utilize Moq for mocking dependencies
- **Assertions**: Use Shouldly for fluent assertions
- **Snapshot Testing**: Use Verify.TUnit for snapshot tests
- **Architecture Tests**: Use ArchUnitNET.TUnit in `tests/BookWorm.ArchTests/`
- **Coverage**: Microsoft.Testing.Extensions.CodeCoverage for test coverage reports
- **Integration Tests**: Use Aspire.Hosting.Testing for service integration tests
- For automated test generation, refer to our [GitHub Copilot test prompts](./prompts/unit-test.prompt.md)

## Pull Request Process

### Pull Request Process

1. **Code Quality**:

   - Follow BookWorm's coding standards and conventions
   - Include comprehensive unit tests for new features and changes
   - Ensure all existing and new tests pass locally before submitting

2. **Documentation**:

   - Update relevant documentation when changing functionality
   - Add code comments for complex logic where necessary
   - Include clear examples for API changes

3. **Submission Requirements**:

   - Link related issues in your PR description using keywords (Fixes #123)
   - Provide a concise description of changes and their purpose
   - Ensure your PR passes all CI/CD pipeline checks, SonarQube analysis, and Snyk security scans

4. **Review Process**:

   - Request reviews from project maintainers
   - Address reviewer feedback promptly
   - Be prepared to make additional changes if requested

5. **Merge Criteria**:
   - PRs require approval from at least one maintainer
   - All automated checks must pass
   - No merge conflicts with the target branch

Here is flowchart of the PR process:

```mermaid
graph TD
		A[Create a PR] --> B{Code Quality}
		B -->|Pass| C[Documentation]
		C -->|Pass| D[Submission Requirements]
		D -->|Pass| E[Review Process]
		E -->|Pass| F[Merge Criteria]
		F -->|Pass| G[PR Merged]
		B -->|Fail| H[Address Feedback]
		C -->|Fail| H
		D -->|Fail| H
		E -->|Fail| H
		H --> B
```

## Need Help?

If you have questions or need assistance, please:

- Check existing issues
- Create a new issue with a detailed description of your problem
- Reach out to the maintainers
