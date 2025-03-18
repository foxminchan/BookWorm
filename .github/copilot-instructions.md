# Instructions for BookWorm

## Project Architecture

BookWorm is a microservices-based application with the following components:

- **BuildingBlocks**: Shared libraries like Constants and SharedKernel
- **Services**: Catalog, Basket, Ordering, Notification, Rating
- **Infrastructure**: AppHost and Gateway
- **Integrations**: HealthChecksUI and Scalar

## Coding Conventions

- Follow DDD (Domain-Driven Design) principles
- Use latest C# features and idioms
- Implement unit tests for business logic
- Maintain service boundaries - avoid cross-service direct dependencies
- Use spaces for indentation with four-spaces per level, unless it is a csproj file, then use two-spaces per level.
- Prefer type declarations over `var` when the type isn't obvious.
- Use `Primary Constructor` for classes with immutable properties.
- Use `Expression-bodied members` for methods and properties.

Example:

```csharp
public class Book
{
		public string Title { get; }
		public string Author { get; }

		public Book(string title, string author)
		{
				Title = title;
				Author = author;
		}
}

public class BookService
{
		public Book GetBook(string title, string author) => new Book(title, author);
}
```

## Service Descriptions

- **Catalog**: Book inventory and metadata management
- **Basket**: Shopping cart functionality
- **Ordering**: Order processing and fulfillment
- **Notification**: User notifications and alerts
- **Rating**: Book reviews and ratings

## Patterns to Follow

- Use CQRS with MediatR when applicable
- Repository pattern for data access
- Domain Events for cross-service communication
- Avoid circular dependencies between services
- Keep services independently deployable

## Testing Guidelines

- Unit test business logic thoroughly
- Name tests using Given_When_Then pattern
- Mock external dependencies
- Test happy paths and edge cases

## Common Tasks

- For adding new API endpoints, follow the existing controller patterns
- When modifying data models, update both entity and DTOs
- Register new services in AppHost project
