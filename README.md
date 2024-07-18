# BookStore: The practical .NET Aspire

## Introduction

<p align="justify">
This is a pet project to demonstrate the practical use of .NET Aspire. The project is a cloud-native application with applying Domain-Driven Design (DDD) and Vertical Slice Methodology.
</p>

<div>
  <a href="https://codespaces.new/foxminchan/BookWorm?quickstart=1">
    <img alt="Open in GitHub Codespaces" src="https://github.com/codespaces/badge.svg">
  </a>
</div>

## The Goals of the Project

- [x] Using `Vertical Slice Architecture` to organize the codebase.
- [x] Using `Domain-Driven Design` to design the domain model.
- [x] Implement the `CQRS` pattern with `MediatR`.
- [x] Using `RabbitMQ` on top `MassTransit` for messaging.
- [x] Using `gRPC` for inter-service communication.
- [x] API versioning
- [x] Health checks
- [x] OpenAPI/Swagger
- [x] AuthN/AuthZ
  - [x] OAuth2 with `Duende IdentityServer`.
  - [x] BFF with `Duende BFF`.
- [x] Observability with `.NET Aspire`.
- [ ] Implement `Frontend` for the application.
  - [ ] Using `Blazor` for the Storefront.
  - [ ] Using `Next.js` for the Admin Panel.
- [ ] Add `MailDev` for local email testing.
- [ ] Output Caching, Response Caching and Distributed Caching with Redis
- [ ] LLMs integration with `Semantic Kernel`
- [ ] Deployment with `Aspirate` on `k3d`.
- [ ] EDA document with `EventCatalog`

## Domain Business & Bounded Contexts - Services Boundaries

- **Catalog**: Display books with pagination and search functionality.
- **Shopping Cart**: Add books to the shopping cart and place an order.
- **Order**: Display orders with pagination and search functionality.
- **Identity**: Register, login, and manage user profile.
- **Notification**: Send email notifications.
- **Rating**: Rate products.

![Domain Business & Bounded Contexts](docs/architechture.png)

## How to run the project

### Prerequisites

- [.NET 8.0 SDK](https://dotnet.microsoft.com/download/dotnet/8.0)
- [Docker](https://www.docker.com/products/docker-desktop)

### Running the project

1. Clone the repository

```bash
git clone git@github.com:foxminchan/BookWorm.git
```

2. Run the project

```bash
dotnet run --project src/BookWorm.AppHost/BookWorm.AppHost.csproj
```

> [!NOTE]
> Ensure that you have Docker running on your machine.

## License

This project is licensed under the MIT License - see the [LICENSE](LICENSE) file for details.
