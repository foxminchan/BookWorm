[![CI](https://github.com/foxminchan/BookWorm/actions/workflows/ci.yaml/badge.svg)](https://github.com/foxminchan/BookWorm/actions/workflows/ci.yaml)
[![Documentation](https://github.com/foxminchan/BookWorm/actions/workflows/docs.yaml/badge.svg)](https://github.com/foxminchan/BookWorm/actions/workflows/docs.yaml)
[![Container Security](https://github.com/foxminchan/BookWorm/actions/workflows/container.yml/badge.svg)](https://github.com/foxminchan/BookWorm/actions/workflows/container.yml)
[![Coverage](https://sonarcloud.io/api/project_badges/measure?project=foxminchan_BookWorm&metric=coverage)](https://sonarcloud.io/summary/new_code?id=foxminchan_BookWorm)
[![Quality Gate Status](https://sonarcloud.io/api/project_badges/measure?project=foxminchan_BookWorm&metric=alert_status)](https://sonarcloud.io/summary/new_code?id=foxminchan_BookWorm)
[![Duplicated Lines (%)](https://sonarcloud.io/api/project_badges/measure?project=foxminchan_BookWorm&metric=duplicated_lines_density)](https://sonarcloud.io/summary/new_code?id=foxminchan_BookWorm)
[![Security Rating](https://sonarcloud.io/api/project_badges/measure?project=foxminchan_BookWorm&metric=security_rating)](https://sonarcloud.io/summary/new_code?id=foxminchan_BookWorm)
[![Reliability Rating](https://sonarcloud.io/api/project_badges/measure?project=foxminchan_BookWorm&metric=reliability_rating)](https://sonarcloud.io/summary/new_code?id=foxminchan_BookWorm)

# 📖 BookWorm: A Practical .NET Aspire Application

## Introduction

<p style="text-align:justify;">
⭐ BookWorm demonstrates the practical implementation of .NET Aspire in a cloud-native application. The project employs Domain-Driven Design (DDD) and Vertical Slice Architecture to organize the codebase effectively.
</p>

<div>
  <a href="https://codespaces.new/foxminchan/BookWorm?quickstart=1" target="_blank">
    <img alt="Open in GitHub Codespaces" src="https://github.com/codespaces/badge.svg">
  </a>
</div>

## 🚀 Technology Stack

<table>
  <tr>
    <td><strong>🏗️ Framework</strong></td>
    <td><strong>🌐 Communication</strong></td>
    <td><strong>💾 Data</strong></td>
    <td><strong>☁️ Cloud & DevOps</strong></td>
  </tr>
  <tr>
    <td>
      • .NET 9 & ASP.NET Core<br>
      • .NET Aspire<br>
      • Vertical Slice Architecture<br>
      • Domain-Driven Design
    </td>
    <td>
      • gRPC<br>
      • REST APIs (OpenAPI)<br>
      • AsyncAPI<br>
      • Event-Driven Architecture
    </td>
    <td>
      • Entity Framework Core<br>
      • Redis (HybridCache)<br>
      • Event Sourcing<br>
      • Outbox/Inbox Patterns
    </td>
    <td>
      • Azure Container Apps<br>
      • Docker & Kubernetes<br>
      • GitHub Actions<br>
      • SonarQube
    </td>
  </tr>
  <tr>
    <td><strong>🤖 AI & ML</strong></td>
    <td><strong>🔐 Security</strong></td>
    <td><strong>📊 Monitoring</strong></td>
    <td><strong>📚 Documentation</strong></td>
  </tr>
  <tr>
    <td>
      • Nomic Embed Text<br>
      • Gemma 3 Chatbot<br>
      • Semantic Kernel<br>
      • Model Context Protocol
    </td>
    <td>
      • Keycloak (OAuth 2.0/OIDC)<br>
      • JWT Authentication<br>
      • RBAC Authorization<br>
      • Security Scanning
    </td>
    <td>
      • Health Checks<br>
      • Distributed Tracing<br>
      • Metrics Collection<br>
      • Load Testing (k6)
    </td>
    <td>
      • Docusaurus (arc42)<br>
      • EventCatalog<br>
      • GitHub Wiki<br>
      • OpenAPI/AsyncAPI
    </td>
  </tr>
</table>

## 🎯 Project Goals

### Core Architecture ✅
- [x] **Cloud-Native Application** - Developed using .NET Aspire for cloud-native scenarios
- [x] **Modern Architecture** - Implemented Vertical Slice Architecture with Domain-Driven Design & CQRS
- [x] **Service Communication** - Enabled service-to-service communication with gRPC

### Microservices Patterns ✅
- [x] **Event Handling** - Utilized outbox and inbox patterns to manage commands and events
- [x] **Orchestration** - Implemented saga patterns for orchestration and choreography
- [x] **Event Sourcing** - Integrated event sourcing for storing domain events
- [x] **Cross-Cutting Concerns** - Implemented a microservices chassis for service infrastructure

### Advanced Features ✅
- [x] **API Management** - Implemented API versioning and feature flags for flexible application management
- [x] **Security** - Set up AuthN/AuthZ with Keycloak
- [x] **Performance** - Implemented caching with HybridCache

### AI Integration ✅
- [x] **Text Processing** - Text embedding with Nomic Embed Text
- [x] **Conversational AI** - Integrated chatbot functionality using Gemma 3
- [x] **AI Tooling** - Standardized AI tooling with Model Context Protocol (MCP)
- [x] **Multi-Agent Systems** - Orchestrated multi-agent workflows using Semantic Kernel
- [x] **Agent Communication** - Enabled agent-to-agent communication via A2A Protocol

### DevOps & Quality ✅
- [x] **CI/CD Pipeline** - Configured with GitHub Actions
- [x] **Documentation** - Created comprehensive documentation:
  - [x] **API Specs** - Used OpenAPI for REST API & AsyncAPI for event-driven endpoints
  - [x] **Architecture Docs** - Utilized EventCatalog for centralized architecture documentation
- [x] **Testing Strategy** - Established comprehensive testing:
  - [x] **Unit Tests** - Conducted service unit tests
  - [x] **Contract Tests** - Implemented contract tests
  - [x] **Architecture Tests** - Established architecture testing strategy
  - [x] **Load Tests** - Performed load testing with k6
  - [ ] **Integration Tests** - Planned integration tests

## Project Architecture

![Project Architecture](assets/BookWorm.png)

## 📚 Documentation Ecosystem

BookWorm features comprehensive documentation across multiple platforms:

| 📖 **Resource** | 🔗 **Link** | 📝 **Description** |
|---|---|---|
| **🏗️ Architecture Docs** | [GitHub Pages](https://foxminchan.github.io/BookWorm/) | Complete arc42 architecture documentation with Docusaurus |
| **📋 Event Catalog** | [Event Documentation](https://bookwormdev.netlify.app/) | Interactive event-driven architecture documentation |
| **📚 GitHub Wiki** | [Project Wiki](https://github.com/foxminchan/BookWorm/wiki) | General project documentation and guides |
| **🔄 API Documentation** | Available when running | OpenAPI (REST) and AsyncAPI (Events) specifications |
| **🎯 ADRs** | [Architecture Decisions](https://foxminchan.github.io/BookWorm/docs/category/architecture-decisions) | Detailed architectural decision records |
| **📊 Quality Metrics** | [SonarCloud](https://sonarcloud.io/summary/new_code?id=foxminchan_BookWorm) | Code quality and security analysis |

## 🚀 Getting Started

### 📋 Prerequisites

Ensure you have the following tools installed:

| Tool | Version | Purpose | Installation Link |
|------|---------|---------|-------------------|
| **.NET SDK** | 9.0 | Core development framework | [Download](https://dotnet.microsoft.com/download/dotnet/9.0) |
| **Node.js** | Latest LTS | Frontend tooling | [Download](https://nodejs.org/en/download/) |
| **Docker** | Latest | Container runtime | [Get Started](https://www.docker.com/get-started) |
| **Bun** | Latest | Fast JavaScript runtime | [Install](https://bun.sh/) |
| **Just** | Latest | Command runner | [Install](https://github.com/casey/just) |
| **Aspire CLI** | Latest | .NET Aspire tooling | [Install Guide](https://learn.microsoft.com/en-us/dotnet/aspire/cli/install) |
| **Gitleaks** | Latest | Secrets detection | [Install](https://gitleaks.io/) |

> [!NOTE]
> **Additional Setup Notes:**
> - 📧 Email services use [SendGrid](https://sendgrid.com/) in production and [Mailpit](https://mailpit.axllent.org/) locally
> - 🍎 Mac with Apple Silicon users need [Rosetta 2](https://support.apple.com/en-us/102527) for grpc-tools compatibility
> - 🐳 Docker Desktop must be running before starting the application

### ⚡ Quick Start

Follow these steps to get BookWorm running locally:

```bash
# 1. Clone the repository
git clone git@github.com:foxminchan/BookWorm.git

# 2. Navigate to the project directory
cd BookWorm

# 3. Run the application (starts all services)
just run
```

> [!TIP]
> **Helpful Commands:**
> - `just help` - View all available commands
> - `just build` - Build the solution
> - `just test` - Run all tests
> - `just clean` - Clean build artifacts

### 🔧 Development Tools

BookWorm includes several development tools accessible when running:

- **🏥 Health Checks UI** - Service health monitoring
- **📊 Metrics Dashboard** - Application performance metrics  
- **🔍 Distributed Tracing** - Request flow visualization
- **📧 Mail Server** - Local email testing with Mailpit
- **🔐 Identity Provider** - Keycloak for authentication testing

### ☁️ Cloud Deployment

Deploy BookWorm to Azure using the [Azure Developer CLI](https://aka.ms/azd):

#### Setup Azure Developer CLI

```bash
# Install Azure Developer CLI (if not already installed)
# Visit: https://aka.ms/azure-dev/install

# Login to your Azure account
azd auth login

# Enable alpha features for bind mounts support
azd config set alpha.azd.operations on
```

#### Initialize and Deploy

```bash
# Initialize azd from the repository root
azd init

# Follow the interactive prompts:
# 1. Select "Use code in the current directory"
# 2. Confirm ".NET (Aspire)" project type
# 3. Choose services to expose (gateway recommended for testing)
# 4. Name your environment

# Create Azure resources and deploy
azd up
```

#### Post-Deployment

After deployment completes:
- 🌐 **Access URL** - `azd` displays the webapp URL for testing
- 🔄 **Updates** - Run `azd up` again after code changes to redeploy
- 📊 **Monitoring** - Use Azure portal for application insights and monitoring

#### Cleanup Resources

```bash
# Remove all Azure resources when done
az group delete --name rg-<your-environment-name> --yes --no-wait
```

> [!NOTE]
> **Deployment Notes:**
> - Initial deployment takes several minutes
> - See [azd troubleshooting guide](https://learn.microsoft.com/azure/developer/azure-developer-cli/troubleshoot?tabs=Browser) for issues
> - Report azd-specific issues to [azure-dev repository](https://github.com/Azure/azure-dev/issues)

### 📖 Documentation Access

All comprehensive project documentation is available through multiple channels:

- **🏗️ [Architecture Documentation](https://foxminchan.github.io/BookWorm/)** - Complete system design using arc42 template
- **📋 [Event Catalog](https://bookwormdev.netlify.app/)** - Interactive event-driven architecture exploration  
- **📚 [GitHub Wiki](https://github.com/foxminchan/BookWorm/wiki)** - Project guides and additional resources
- **📊 [Quality Dashboard](https://sonarcloud.io/summary/new_code?id=foxminchan_BookWorm)** - Code quality metrics and analysis

> [!TIP]  
> Start with the [Architecture Documentation](https://foxminchan.github.io/BookWorm/) for a comprehensive system overview, then explore the [Event Catalog](https://bookwormdev.netlify.app/) to understand the event flows.

## 🤝 Contribution

We welcome contributions from the community! Thanks to all [contributors](https://github.com/foxminchan/BookWorm/graphs/contributors) - your help is greatly appreciated! 🎉

### How to Contribute

1. **📖 Read Guidelines** - Review our [contribution guidelines](./.github/CONTRIBUTING.md)  
2. **🤝 Follow Code of Conduct** - Adhere to our [code of conduct](./.github/CODE-OF-CONDUCT.md)
3. **🔄 Development Process** - Follow Git Flow branching model
4. **🧪 Testing** - Ensure your changes include appropriate tests
5. **📝 Documentation** - Update relevant documentation

### Areas for Contribution

- 🐛 **Bug Fixes** - Help resolve open issues
- ✨ **Feature Development** - Implement new capabilities  
- 📚 **Documentation** - Improve or expand documentation
- 🧪 **Testing** - Add test coverage and scenarios
- 🔍 **Code Review** - Review pull requests from other contributors

## 💬 Support

### Community Support

- ⭐ **Star the Project** - If you find BookWorm helpful, please give it a star!
- 🐛 **Report Issues** - [Create an issue](https://github.com/foxminchan/BookWorm/issues/new/choose) for bugs or feature requests
- 💭 **Discussions** - Join our [GitHub Discussions](https://github.com/foxminchan/BookWorm/discussions) for questions and ideas
- 📖 **Documentation** - Check our comprehensive [documentation ecosystem](#-documentation-ecosystem)

### Getting Help

| 📝 **Type** | 🔗 **Where to Go** | ⏱️ **Response Time** |
|-------------|-------------------|---------------------|
| **🐛 Bug Reports** | [GitHub Issues](https://github.com/foxminchan/BookWorm/issues) | Usually within 24-48 hours |
| **💡 Feature Requests** | [GitHub Issues](https://github.com/foxminchan/BookWorm/issues) | Weekly review cycle |
| **❓ Questions** | [GitHub Discussions](https://github.com/foxminchan/BookWorm/discussions) | Community-driven |
| **📚 Documentation** | [Architecture Docs](https://foxminchan.github.io/BookWorm/) | Always available |

## License

This project is licensed under the MIT License. See the [LICENSE](LICENSE) file for details.
