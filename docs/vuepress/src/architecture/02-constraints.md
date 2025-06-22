---
category:
  - Architecture Documentation
tag:
  - arc42
---

# 2. Architecture Constraints

## 2.1 Technical Constraints

### Platform and Technology Stack

| Constraint               | Description                        | Rationale                                              |
| ------------------------ | ---------------------------------- | ------------------------------------------------------ |
| **.NET 9**               | Latest .NET framework              | Access to newest features and performance improvements |
| **.NET Aspire**          | Cloud-native application framework | Simplified cloud-native development and deployment     |
| **Azure Container Apps** | Primary deployment target          | Managed container service with auto-scaling            |
| **Docker/Podman**        | Containerization platform          | Consistent deployment across environments              |

### Development Constraints

| Constraint                      | Description                              | Impact                                               |
| ------------------------------- | ---------------------------------------- | ---------------------------------------------------- |
| **Domain-Driven Design**        | Mandatory architectural approach         | Influences service boundaries and model design       |
| **Vertical Slice Architecture** | Required code organization               | Affects project structure and feature implementation |
| **Event-Driven Architecture**   | Required communication pattern           | Impacts service interaction and data consistency     |
| **CQRS Pattern**                | Command Query Responsibility Segregation | Separates read and write operations                  |
| **OpenAPI/AsyncAPI**            | API documentation standards              | All APIs must be properly documented                 |

## 2.2 Organizational Constraints

### Development Process

- **Open Source Development**: Public repository with community contributions
- **MIT License**: Permissive licensing for maximum flexibility
- **GitHub Actions**: CI/CD pipeline for automated testing and deployment
- **Code Quality**: SonarCloud integration for code quality metrics
- **Testing Strategy**: Comprehensive testing including unit, integration, and load testing

### Documentation Requirements

- **Architecture Documentation**: arc42 template compliance
- **API Documentation**: OpenAPI for REST APIs, AsyncAPI for events
- **Event Catalog**: Centralized event-driven architecture documentation
- **Deployment Guides**: Comprehensive deployment instructions

## 2.3 Conventions and Standards

### Coding Standards

- **EditorConfig**: Consistent code formatting across the project
- **C# Coding Conventions**: Microsoft C# coding standards
- **Nullable Reference Types**: Enabled for better null safety
- **Pattern Matching**: Preferred over traditional conditional logic

### Architecture Patterns

| Pattern            | Application                   | Justification                           |
| ------------------ | ----------------------------- | --------------------------------------- |
| **Microservices**  | Service decomposition         | Scalability and independent deployment  |
| **Event Sourcing** | Domain events storage         | Audit trail and temporal queries        |
| **Outbox Pattern** | Reliable event publishing     | Ensures message delivery consistency    |
| **Inbox Pattern**  | Idempotent message processing | Prevents duplicate message processing   |
| **Saga Pattern**   | Distributed transactions      | Manages long-running business processes |

## 2.4 Infrastructure Constraints

### Deployment Environment

- **Azure Cloud**: Primary cloud provider
- **Container-based**: All services deployed as containers
- **Managed Services**: Preference for managed Azure services
- **Infrastructure as Code**: Automated infrastructure provisioning

### Security Requirements

- **Authentication**: Keycloak for identity management
- **Authorization**: Role-based access control (RBAC)
- **HTTPS Only**: All communication encrypted in transit
- **Secret Management**: Azure Key Vault for sensitive configuration
- **Security Scanning**: Automated vulnerability scanning

## 2.5 External Dependencies

### Third-Party Services

| Service            | Purpose                        | Constraint Impact                      |
| ------------------ | ------------------------------ | -------------------------------------- |
| **Keycloak**       | Identity and access management | Authentication architecture dependency |
| **SendGrid**       | Email delivery service         | Email functionality limitation         |
| **Azure Services** | Cloud infrastructure           | Platform-specific implementation       |
| **AI Models**      | Nomic Embed Text, Gemma 3      | GPU requirements for local development |

### Hardware Requirements

- **GPU Support**: Required for AI components in local development
- **Container Runtime**: Docker or Podman for local development
- **Memory**: Minimum 8GB RAM for full local development setup
- **Storage**: SSD recommended for optimal performance
