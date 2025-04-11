[![CI](https://github.com/foxminchan/BookWorm/actions/workflows/ci.yaml/badge.svg)](https://github.com/foxminchan/BookWorm/actions/workflows/ci.yaml)
[![Coverage](https://sonarcloud.io/api/project_badges/measure?project=foxminchan_BookWorm&metric=coverage)](https://sonarcloud.io/summary/new_code?id=foxminchan_BookWorm)
[![Duplicated Lines (%)](https://sonarcloud.io/api/project_badges/measure?project=foxminchan_BookWorm&metric=duplicated_lines_density)](https://sonarcloud.io/summary/new_code?id=foxminchan_BookWorm)
[![Netlify Status](https://api.netlify.com/api/v1/badges/ff82b0cb-bbb5-4d49-b326-e4792d673420/deploy-status)](https://app.netlify.com/sites/bookwormdev/deploys)

# 📖 BookWorm: A Practical .NET Aspire Application

## Introduction

<p align="justify">
⭐ BookWorm demonstrates the practical implementation of .NET Aspire in a cloud-native application. The project employs Domain-Driven Design (DDD) and Vertical Slice Architecture to organize the codebase effectively.
</p>

<div>
  <a href="https://codespaces.new/foxminchan/BookWorm?quickstart=1">
    <img alt="Open in GitHub Codespaces" src="https://github.com/codespaces/badge.svg">
  </a>
</div>

## Project Goals

- [x] Build a cloud-native application with `.NET Aspire`
- [x] Implement `Vertical Slice Architecture` for codebase organization
- [x] Apply `Domain-Driven Design` principles
- [x] Implement `CQRS` pattern with `MediatR`
- [x] Utilize `RabbitMQ` with `MassTransit` for messaging
- [x] Implement `gRPC` for service-to-service communication
- [x] Implement `Saga` patterns (Orchestration & Choreography) with `MassTransit`
- [x] Implement `Event Sourcing`
- [x] Implement `Outbox Pattern` and `Inbox Pattern` for messaging
- [x] Support API versioning
- [x] Implement comprehensive health checks
- [x] Enable feature flags to control application behavior
- [x] Implement AuthN/AuthZ with `Keycloak`
- [x] Enable observability with `.NET Aspire`
- [x] Integrate `Mailpit` for local email testing
- [x] Integrate with AI components
  - [x] Text embedding with `Nomic Embed Text`
  - [x] Support hybrid search with `Qdrant`
  - [x] Chatbot integration with `DeepSeek R1`
- [x] Configure CI/CD with `GitHub Actions`
- [x] Deploy with `Aspirate` on `k3d`
- [x] Create documentation for the project
  - [x] Use `OpenAPI/Scalar` for REST API documentation
  - [x] Provide `AsyncAPI` for event-driven endpoints
  - [x] Use `EventCatalog` for event inventory and discovery
  - [ ] Integrate `Backstage` for developer portal experience
- [x] Implement testing strategy
  - [x] Service unit tests
  - [ ] Integration tests

## Project Architecture

BookWorm is structured as a microservices-based application with the following services:

- **Catalog**: Book inventory and metadata management.
- **Basket**: Shopping cart functionality.
- **Ordering**: Order processing and fulfillment.
- **Notification**: Handles email notifications as a Worker Service.
- **Rating**: Book review and rating system.
- **Finance**: Orchestration service for basket reservation and order processing.

![Domain Architecture](assets/architecture.png)

## Getting Started

### Prerequisites

- A device with an Nvidia GPU
- [.NET 9.0 SDK](https://dotnet.microsoft.com/download/dotnet/9.0)
- [Gitleaks](https://gitleaks.io/)
- [Docker](https://www.docker.com/get-started) or [Podman](https://podman-desktop.io/)
- [k3d](https://k3d.io/) & [k9s](https://k9scli.io/) & [helm](https://helm.sh/)
- [Azure CLI](https://docs.microsoft.com/en-us/cli/azure/install-azure-cli)

### System Requirements

> [!IMPORTANT]
>
> - **AI Components**: This project uses [DeepSeek R1](https://ollama.com/library/deepseek-r1) for chatbot integration and [Nomic Embed Text](https://ollama.com/library/nomic-embed-text) for text embedding, requiring an **Nvidia GPU** for local development.
> - **Email Services**: Production uses [SendGrid](https://sendgrid.com/), while local development uses [Mailpit](https://mailpit.axllent.org/) for email testing.

### Run the Application

1. Clone the repository

   ```bash
   git clone git@github.com:foxminchan/BookWorm.git
   ```

2. Change directory to the repository

   ```bash
   cd BookWorm
   ```

3. Run the application

   ```bash
   make run
   ```

> [!WARNING]
> Docker must be running on your machine before starting the application.

## Contributing

Please read [CONTRIBUTING.md](./.github/CONTRIBUTING.md) for details on our code of conduct, and the process for submitting pull requests to us. Thank you for contributing to BookWorm!

## Support

- If you like this project, please give it a ⭐ star.
- If you have any issues or feature requests, please [create an issue](https://github.com/foxminchan/BookWorm/issues/new/choose).

## License

This project is licensed under the MIT License - see the [LICENSE](LICENSE) file for details.
