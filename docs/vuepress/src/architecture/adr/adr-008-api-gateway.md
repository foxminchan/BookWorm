---
category:
  - Architecture Decisions Records
tag:
  - ADR
---

# ADR-008: API Gateway Pattern Implementation

## Status

**Accepted** - December 2024

## Context

Multiple microservices need a unified entry point for clients with cross-cutting concerns like authentication, rate limiting, and request routing.

## Decision

Implement an API Gateway using YARP (Yet Another Reverse Proxy) in ASP.NET Core.

## Rationale

- **Single Entry Point**: Simplified client integration
- **Cross-cutting Concerns**: Centralized authentication, logging, rate limiting
- **Request Routing**: Dynamic routing to appropriate services
- **Load Balancing**: Built-in load balancing capabilities
- **.NET Native**: YARP provides high-performance reverse proxy for .NET

## Implementation

### Gateway Configuration

```json
{
  "ReverseProxy": {
    "Routes": {
      "catalog-route": {
        "ClusterId": "catalog-cluster",
        "Match": {
          "Path": "/api/catalog/{**catch-all}"
        },
        "Transforms": [{ "PathPattern": "/api/{**catch-all}" }]
      },
      "ordering-route": {
        "ClusterId": "ordering-cluster",
        "Match": {
          "Path": "/api/orders/{**catch-all}"
        }
      }
    },
    "Clusters": {
      "catalog-cluster": {
        "Destinations": {
          "catalog-api": {
            "Address": "https+http://catalog-api"
          }
        }
      },
      "ordering-cluster": {
        "Destinations": {
          "ordering-api": {
            "Address": "https+http://ordering-api"
          }
        }
      }
    }
  }
}
```

## Consequences

### Positive

- Simplified client development
- Centralized security enforcement
- Consistent API versioning
- Built-in monitoring and logging

### Negative

- Potential bottleneck
- Additional latency
- Single point of failure
- Gateway complexity

## Related Decisions

- [ADR-001: Microservices Architecture](adr-001-microservices-architecture.md)
- [ADR-005: Keycloak for Identity Management](adr-005-keycloak-identity.md)
