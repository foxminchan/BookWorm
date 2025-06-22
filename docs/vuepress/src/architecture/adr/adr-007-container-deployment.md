---
category:
  - Architecture Decisions Records
tag:
  - ADR
---

# ADR-007: Container-First Deployment Strategy

## Status

**Accepted** - December 2024

## Context

The application needs to be cloud-native with consistent deployment across environments and easy scaling capabilities.

## Decision

Adopt a container-first deployment strategy using Docker containers and Azure Container Apps.

## Rationale

- **Consistency**: Same artifacts across all environments
- **Scalability**: Horizontal scaling with container orchestration
- **Cloud-Native**: Aligned with modern cloud deployment practices
- **Azure Integration**: Azure Container Apps provides managed container hosting
- **Developer Experience**: Simplified local development with Docker Compose

## Implementation

### Infrastructure as Code

```bicep
@description('Container Apps Environment')
param environmentName string = 'bookworm-env'
param location string = resourceGroup().location

resource containerEnvironment 'Microsoft.App/managedEnvironments@2024-03-01' = {
  name: environmentName
  location: location
  properties: {
    workloadProfiles: [
      {
        name: 'Consumption'
        workloadProfileType: 'Consumption'
      }
    ]
  }
}

resource containerApp 'Microsoft.App/containerApps@2024-03-01' = {
  name: 'bookworm-api'
  location: location
  properties: {
    managedEnvironmentId: containerEnvironment.id
    template: {
      containers: [
        {
          name: 'api'
          image: 'bookworm.azurecr.io/catalog-api:latest'
          resources: {
            cpu: json('0.25')
            memory: '0.5Gi'
          }
        }
      ]
      scale: {
        minReplicas: 1
        maxReplicas: 10
      }
    }
  }
}
```

## Consequences

### Positive

- Environment consistency
- Easy scaling and deployment
- Resource isolation
- Simplified CI/CD pipelines

### Negative

- Container orchestration complexity
- Additional infrastructure overhead
- Learning curve for container technologies

## Related Decisions

- [ADR-003: .NET Aspire for Cloud-Native Development](adr-003-aspire-cloud-native.md)
