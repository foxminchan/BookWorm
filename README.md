[<img src="./assets/Logo.svg" alt="BookWorm" width=80>](https://foxminchan.github.io/BookWorm/)

# BookWorm

[![BookWorm CI (.NET)](https://github.com/foxminchan/BookWorm/actions/workflows/dotnet-ci.yaml/badge.svg?branch=main)](https://github.com/foxminchan/BookWorm/actions/workflows/dotnet-ci.yaml)
[![BookWorm CI (Keycloakify)](https://github.com/foxminchan/BookWorm/actions/workflows/keycloak-ci.yaml/badge.svg?branch=main)](https://github.com/foxminchan/BookWorm/actions/workflows/keycloak-ci.yaml)
[![Coverage](https://sonarcloud.io/api/project_badges/measure?project=foxminchan_BookWorm&metric=coverage)](https://sonarcloud.io/summary/new_code?id=foxminchan_BookWorm)
[![Duplicated Lines (%)](https://sonarcloud.io/api/project_badges/measure?project=foxminchan_BookWorm&metric=duplicated_lines_density)](https://sonarcloud.io/summary/new_code?id=foxminchan_BookWorm)

<div>
  <a href="https://codespaces.new/foxminchan/BookWorm?quickstart=1" target="_blank">
    <img alt="Open in GitHub Codespaces" src="https://github.com/codespaces/badge.svg">
  </a>
</div>

## Introduction

<p align="justify">
  ⭐ BookWorm showcases .NET Aspire in a cloud-native application with AI integration. Built with DDD and VSA, it features multi-agent orchestration and standardized AI tooling through MCP with A2A Protocol support.
</p>

> [!WARNING]
>
> **Disclaimer**: This example is for demo use only. It’s not production-ready and may omit important features.

## Project Goals

- [x] Developed a cloud-native application using .NET Aspire
- [x] Implemented Vertical Slice Architecture with Domain-Driven Design & CQRS
- [x] Enabled service-to-service communication with gRPC
- [x] Incorporated various microservices patterns
  - [x] Utilized outbox and inbox patterns to manage commands and events
  - [x] Implemented saga patterns for orchestration and choreography
  - [x] Integrated event sourcing for storing domain events
  - [x] Implemented a microservices chassis for cross-cutting concerns and service infrastructure
- [x] Implemented API versioning and feature flags for flexible application management
- [x] Set up AuthN/AuthZ with Keycloak
- [x] Implemented caching with HybridCache
- [x] Incorporated AI components:
  - [x] Text embedding with Embedding Gemma
  - [x] Integrated chatbot functionality using Gemma 3
  - [x] Standardized AI tooling with Model Context Protocol (MCP)
  - [x] Orchestrated multi-agent workflows using Agent Framework
  - [x] Enabled agent-to-agent communication via A2A Protocol
- [x] Configured CI/CD with GitHub Actions
- [x] Created comprehensive documentation:
  - [x] Used OpenAPI for REST API & AsyncAPI for event-driven endpoints
  - [x] Utilized EventCatalog for centralized architecture documentation
- [x] Established a testing strategy:
  - [x] Conducted service unit tests
  - [x] Implemented snapshot tests
  - [x] Established architecture testing strategy
  - [x] Performed load testing with k6
  - [ ] Planned integration tests

## Project Architecture

![Project Architecture](assets/BookWorm.png)

## Getting Started

### Prerequisites

- [.NET 10.0 SDK](https://dotnet.microsoft.com/download/dotnet/10.0)
- [Node.js](https://nodejs.org/en/download/)
- [Docker](https://www.docker.com/get-started)
- [Gitleaks](https://gitleaks.io/)
- [Bun](https://bun.sh/)
- [Just](https://github.com/casey/just)
- [Aspire CLI](https://learn.microsoft.com/en-us/dotnet/aspire/cli/install)
- [Azure CLI](https://learn.microsoft.com/cli/azure/install-azure-cli)
- Optional: [Spec-Kit](https://github.com/github/spec-kit)
- Optional: [GitHub Copilot CLI](https://github.com/github/copilot-cli)

> [!NOTE]
>
> - 📧 Email services use [SendGrid](https://sendgrid.com/) in production and [Mailpit](https://mailpit.axllent.org/) locally
> - 🍎 Mac with Apple Silicon users need [Rosetta 2](https://support.apple.com/en-us/102527) for grpc-tools compatibility
> - 🐳 Docker Desktop must be running before starting the application

### Run the Application

Follow these steps to get BookWorm running locally:

```sh
# 1. Clone the repository
git clone git@github.com:foxminchan/BookWorm.git

# 2. Navigate to the project directory
cd BookWorm

# 3. Run the application (starts all services)
just run
```

> [!TIP]
>
> - Run `just help` to see all available commands.

### Deploy the Application

To deploy BookWorm to Azure Container Apps, follow these steps:

1. **Authenticate with Azure**:

```sh
az login
```

2. **Deploy the application**:

```sh
aspire deploy
```

3. **Verify the deployment**:

After deployment completes, get the application URL:

```sh
az containerapp show --name <app-name> --resource-group <resource-group> --query properties.configuration.ingress.fqdn --output tsv
```

Replace `<app-name>` and `<resource-group>` with the values you specified during deployment.

4. **Clean up resources**:

To remove all deployed resources and avoid charges:

```sh
az group delete --name <resource-group> --yes --no-wait
```

### Documentation

For comprehensive project documentation, visit our [GitHub Wiki](https://github.com/foxminchan/BookWorm/wiki).

## Contribution

Thanks to all [contributors](https://github.com/foxminchan/BookWorm/graphs/contributors), your help is greatly appreciated!

Contributions are welcome! Please read the [contribution guidelines](./.github/CONTRIBUTING.md) and [code of conduct](./.github/CODE-OF-CONDUCT.md) to learn how to participate.

## Support

- If you like this project, please give it a ⭐ star.
- If you have any issues or feature requests, please [create an issue](https://github.com/foxminchan/BookWorm/issues/new/choose).

## License

This project is licensed under the MIT License. See the [LICENSE](LICENSE) file for details.
