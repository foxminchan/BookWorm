# BookStore: The practical .NET Aspire

## Introduction

This is a pet project to demonstrate the practical use of .NET Aspire. The project is a cloud-native application with applying Domain-Driven Design (DDD) and Vertical Slice Methodology. The project is a simple online bookstore with the following features:

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

## License

This project is licensed under the MIT License - see the [LICENSE](LICENSE) file for details.
