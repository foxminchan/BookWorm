[![CI](https://github.com/foxminchan/BookWorm/actions/workflows/ci.yaml/badge.svg)](https://github.com/foxminchan/BookWorm/actions/workflows/ci.yaml)
[![Coverage](https://sonarcloud.io/api/project_badges/measure?project=foxminchan_BookWorm&metric=coverage)](https://sonarcloud.io/summary/new_code?id=foxminchan_BookWorm)
[![Duplicated Lines (%)](https://sonarcloud.io/api/project_badges/measure?project=foxminchan_BookWorm&metric=duplicated_lines_density)](https://sonarcloud.io/summary/new_code?id=foxminchan_BookWorm)

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

- [x] Developed a cloud-native application using .NET Aspire
- [x] Implemented Vertical Slice Architecture with Domain-Driven Design & CQRS
- [x] Enabled service-to-service communication with gRPC
- [x] Incorporated various microservices patterns
  - [x] Utilized outbox and inbox patterns to manage commands and events
  - [x] Implemented saga patterns for orchestration and choreography
  - [x] Integrated event sourcing for storing domain events
  - [x] Implemented a microservices chassis for cross-cutting concerns and service infrastructure
- [x] Implemented API versioning and feature flags for flexible application management
- [x] Set up AuthN/AuthZ with Keycloak (see [here](./src/Aspire/BookWorm.AppHost/Container/keycloak/README.md))
- [x] Implemented caching with HybridCache
- [x] Incorporated AI components:
  - [x] Text embedding with Nomic Embed Text
  - [x] Integrated chatbot functionality using Gemma 3
  - [x] Standardized AI tooling with Model Context Protocol (MCP)
  - [x] Orchestrated multi-agent workflows using Semantic Kernel
  - [x] Enabled agent-to-agent communication via A2A Protocol
- [x] Configured CI/CD with GitHub Actions
- [x] Created comprehensive documentation:
  - [x] Used OpenAPI for REST API & AsyncAPI for event-driven endpoints
  - [x] Utilized EventCatalog for centralized architecture documentation
- [x] Established a testing strategy:
  - [x] Conducted service unit tests
  - [x] Established architecture testing strategy
  - [x] Performed load testing with k6 (see [testing suite](./src/Aspire/BookWorm.AppHost/Container/k6/README.md))
  - [ ] Planned integration tests

## Project Architecture

![Project Architecture](assets/architecture.png)

## Getting Started

### Prerequisites

- [.NET 9.0 SDK](https://dotnet.microsoft.com/download/dotnet/9.0)
- [Docker](https://www.docker.com/get-started)
- [Node.js](https://nodejs.org/en/download/)
- [Bun](https://bun.sh/)
- [Just](https://github.com/casey/just)
- [Gitleaks](https://gitleaks.io/)

> [!NOTE]
>
> Email services use [SendGrid](https://sendgrid.com/) in production and [Mailpit](https://mailpit.axllent.org/) locally.

### Run the Application

1. **Clone the repository**

   ```bash
   git clone git@github.com:foxminchan/BookWorm.git
   ```

2. **Navigate to the project directory**

   ```bash
   cd BookWorm
   ```

3. **Run the application**

   ```bash
   just run
   ```

> [!IMPORTANT]
>
> - Docker must be running on your machine before starting the application.
> - For GPU support with AI components, install GPU drivers and run `just gpu 1`.
> - Run `just help` to see all available commands.

### Deploy the application

You can use the [Azure Developer CLI](https://aka.ms/azd) to run this project on Azure with only a few commands. Follow the next instructions:

- Install the latest or update to the latest [Azure Developer CLI (azd)](https://aka.ms/azure-dev/install).
- Log in `azd` (if you haven't done it before) to your Azure account:

```sh
azd auth login
```

- Enable the `azd` alpha features to support bind mounts:

```sh
azd config set alpha.azd.operations on
```

- Initialize `azd` from the root of the repository.

```sh
azd init
```

- During initialization, you will be prompted to select the project type and services to expose. Follow these steps:

  - Select `Use code in the current directory`. Azd will automatically detect the .NET Aspire project.
  - Confirm `.NET (Aspire)` and continue.
  - Select which services to expose to the Internet (exposing `gateway` is enough to test the sample).
  - Finalize the initialization by giving a name to your environment.

- Create Azure resources and deploy the sample by running:

```sh
azd up
```

> [!NOTE]
>
> - The operation takes a few minutes the first time it is ever run for an environment.
> - At the end of the process, `azd` will display the `url` for the webapp. Follow that link to test the sample.
> - You can run `azd up` after saving changes to the sample to re-deploy and update the sample.
> - Report any issues to [azure-dev](https://github.com/Azure/azure-dev/issues) repo.
> - [FAQ and troubleshoot](https://learn.microsoft.com/azure/developer/azure-developer-cli/troubleshoot?tabs=Browser) for azd.

- For cleaning up the resources created by `azd`, run:

```sh
az group delete --name rg-<your-environment-name> --yes --no-wait
```

### Event-Driven Architecture

Explore our [event catalog](https://bookwormdev.netlify.app/) for messaging patterns and API details.

### Architecture Documentation

For comprehensive architecture documentation, refer to the [arc42 documentation](https://foxminchan.github.io/BookWorm/).

## Contributing

Contributions are welcome! Please read the [contribution guidelines](./.github/CONTRIBUTING.md) and [code of conduct](./.github/CODE-OF-CONDUCT.md) to learn how to participate.

## Support

- If you like this project, please give it a ⭐ star.
- If you have any issues or feature requests, please [create an issue](https://github.com/foxminchan/BookWorm/issues/new/choose).

## License

This project is licensed under the MIT License - see the [LICENSE](LICENSE) file for details.
