---
category:
  - Architecture Documentation
tag:
  - arc42
---

# 7. Deployment View

## 7.1 Infrastructure Overview

BookWorm is designed for cloud-native deployment on Azure Container Apps (ACA), providing managed container hosting with automatic scaling, load balancing, and integrated monitoring.

### High-Level Deployment Architecture

@startuml BookWorm-Deployment
!define AZURE_CLOUD_COLOR #0078D4
!define CONTAINER_COLOR #E1F5FE
!define SERVICE_COLOR #F3E5F5
!define DATA_COLOR #E8F5E8
!define EXTERNAL_COLOR #FFF3E0

skinparam backgroundColor White
skinparam nodeBackgroundColor AZURE_CLOUD_COLOR
skinparam artifactBackgroundColor SERVICE_COLOR
skinparam databaseBackgroundColor DATA_COLOR
skinparam cloudBackgroundColor EXTERNAL_COLOR

cloud "Azure Cloud" as AzureCloud {
node "Azure Container Apps Environment" as ACAEnv {
package "Frontend Tier" as FrontendTier {
artifact "🖥️ Web Application\nSPA React/Blazor" as WebApp
artifact "⚙️ Admin Dashboard\nManagement Interface" as AdminUI
}

        package "API Gateway Tier" as GatewayTier {
            artifact "🌐 API Gateway\nYARP Reverse Proxy" as Gateway
        }

        package "Microservices Tier" as MicroservicesTier {
            artifact "📚 Catalog Service\n2-10 replicas" as CatalogAPI
            artifact "🛒 Ordering Service\n2-5 replicas" as OrderingAPI
            artifact "🛍️ Basket Service\n1-3 replicas" as BasketAPI
            artifact "⭐ Rating Service\n1-3 replicas" as RatingAPI
            artifact "💬 Chat Service\n1-5 replicas" as ChatAPI
        }

        package "Infrastructure Services" as InfraServices {
            artifact "🔐 Keycloak\nIdentity Provider" as Keycloak
            artifact "📡 Event Bus\nRabbitMQ/Service Bus" as EventBus
        }
    }

    package "Data Services" as DataServices {
        database "🗄️ Azure Database\nPostgreSQL Flexible" as PostgreSQL
        database "⚡ Azure Cache\nRedis" as Redis
        database "💾 Azure Blob Storage\nStatic Assets" as Storage
    }

    package "External Services" as ExternalServices {
        cloud "🌐 Azure CDN\nContent Delivery" as CDN
        node "🔑 Azure Key Vault\nSecrets Management" as KeyVault
        node "📊 Azure Monitor\nObservability" as Monitor
    }

}

cloud "External APIs" as ExternalAPIs {
cloud "📧 SendGrid API" as SendGrid
cloud "🤖 AI Model APIs\nNomic, Gemma" as AIServices
}

' Frontend connections
WebApp --> Gateway : HTTPS
AdminUI --> Gateway : HTTPS

' Gateway to microservices
Gateway --> CatalogAPI : HTTP
Gateway --> OrderingAPI : HTTP
Gateway --> BasketAPI : HTTP
Gateway --> RatingAPI : HTTP
Gateway --> ChatAPI : HTTP/WebSocket

' Microservices to databases
CatalogAPI --> PostgreSQL : SQL
OrderingAPI --> PostgreSQL : SQL
BasketAPI --> Redis : Redis Protocol
RatingAPI --> PostgreSQL : SQL

' Event Bus connections
CatalogAPI --> EventBus : AMQP
OrderingAPI --> EventBus : AMQP
RatingAPI --> EventBus : AMQP
ChatAPI --> EventBus : AMQP

' Authentication
Gateway --> Keycloak : OIDC

' External API connections
ChatAPI --> SendGrid : HTTPS
CatalogAPI --> AIServices : HTTPS

' Azure services
WebApp --> CDN : HTTPS
CatalogAPI --> Storage : HTTPS
Gateway --> KeyVault : HTTPS
CatalogAPI --> Monitor : HTTPS

@enduml

## 7.2 Infrastructure as Code with Bicep

### Service Scaling Configuration

| Service          | Min Replicas | Max Replicas | Scaling Rules                                    |
| ---------------- | ------------ | ------------ | ------------------------------------------------ |
| **Catalog API**  | 2            | 10           | CPU > 70%, Memory > 80%, HTTP requests > 100/min |
| **Ordering API** | 2            | 5            | CPU > 80%, HTTP requests > 50/min                |
| **Basket API**   | 1            | 3            | CPU > 70%, HTTP requests > 200/min               |
| **Rating API**   | 1            | 3            | HTTP requests > 30/min                           |
| **Chat API**     | 1            | 5            | WebSocket connections > 100, CPU > 70%           |
| **API Gateway**  | 2            | 5            | CPU > 60%, HTTP requests > 500/min               |

## 7.4 Data Layer Deployment

### Database Configuration

#### PostgreSQL (Primary Database)

```bicep
resource postgres 'Microsoft.DBforPostgreSQL/flexibleServers@2024-08-01' = {
  name: take('postgres-${uniqueString(resourceGroup().id)}', 63)
  location: location
  properties: {
    administratorLogin: administratorLogin
    administratorLoginPassword: administratorLoginPassword
    authConfig: {
      activeDirectoryAuth: 'Disabled'
      passwordAuth: 'Enabled'
    }
    availabilityZone: '1'
    backup: {
      backupRetentionDays: 7
      geoRedundantBackup: 'Disabled'
    }
    highAvailability: {
      mode: 'Disabled'
    }
    storage: {
      storageSizeGB: 32
    }
    version: '16'
  }
  sku: {
    tier: 'GeneralPurpose'
  }
  tags: {
    'aspire-resource-name': 'postgres'
    Environment: 'Production'
    Projects: 'BookWorm'
  }
}
```

#### Redis Cache

```bicep
resource redis 'Microsoft.Cache/redis@2024-03-01' = {
  name: take('redis-${uniqueString(resourceGroup().id)}', 63)
  location: location
  properties: {
    sku: {
      name: 'Basic'
      family: 'C'
      capacity: 1
    }
    enableNonSslPort: false
    minimumTlsVersion: '1.2'
  }
  tags: {
    'aspire-resource-name': 'redis'
    Environment: 'Production'
    Projects: 'BookWorm'
  }
}
```

## 7.5 Network and Security Configuration

### Network Security

@startuml Network-Security
!define INTERNET_COLOR #FFECB3
!define AZURE_FRONT_DOOR_COLOR #E3F2FD
!define PUBLIC_SUBNET_COLOR #E8F5E8
!define PRIVATE_SUBNET_COLOR #FFF3E0
!define DATA_SUBNET_COLOR #FCE4EC

skinparam backgroundColor White
skinparam cloudBackgroundColor INTERNET_COLOR
skinparam nodeBackgroundColor AZURE_FRONT_DOOR_COLOR
skinparam packageBackgroundColor PUBLIC_SUBNET_COLOR

cloud "Internet" as Internet {
actor "👥 Users" as Users
actor "👤 Administrators" as AdminUsers
}

node "Azure Front Door" as AzureFrontDoor {
component "🛡️ Web Application Firewall" as WAF
component "🌐 Content Delivery Network" as CDN
}

package "Container Apps Environment" as ContainerAppsEnv {
package "Public Subnet" as PublicSubnet {
component "🌐 API Gateway" as Gateway
component "🖥️ Web Application" as WebApp
}

    package "Private Subnet" as PrivateSubnet {
        component "🔧 Microservices" as Services
        component "⚖️ Internal Load Balancer" as InternalLB
    }

}

package "Data Subnet" as DataSubnet {
database "🗄️ Databases" as Databases
database "⚡ Cache" as Cache
}

' Network flow connections
Users --> WAF : HTTPS
AdminUsers --> WAF : HTTPS
WAF --> CDN : Filtered Traffic
CDN --> Gateway : Load Balanced
CDN --> WebApp : Static Content
Gateway --> InternalLB : Internal Routing
InternalLB --> Services : Service Discovery
Services --> Databases : Secure Connection
Services --> Cache : Redis Protocol

@enduml

### Security Configuration

| Component       | Security Measure               | Implementation                  |
| --------------- | ------------------------------ | ------------------------------- |
| **API Gateway** | TLS termination, Rate limiting | Azure Container Apps ingress    |
| **Services**    | mTLS, JWT validation           | .NET Aspire security middleware |
| **Databases**   | Private endpoints, SSL only    | Azure private link              |
| **Secrets**     | Key Vault integration          | Managed identity authentication |
| **Network**     | NSG rules, Private DNS         | Azure VNET configuration        |

## 7.6 CI/CD Pipeline

### Deployment Pipeline

@startuml CICD-Pipeline
!define SOURCE_COLOR #E8F5E8
!define BUILD_COLOR #E3F2FD
!define REGISTRY_COLOR #FFF3E0
!define DEPLOY_COLOR #F3E5F5
!define MONITOR_COLOR #FFECB3

skinparam backgroundColor White
skinparam packageBackgroundColor SOURCE_COLOR
skinparam nodeBackgroundColor BUILD_COLOR
skinparam databaseBackgroundColor REGISTRY_COLOR
skinparam cloudBackgroundColor DEPLOY_COLOR

package "Source Control" as SourceControl {
component "📂 GitHub Repository" as GitHub
}

node "Build Pipeline" as BuildPipeline {
component "🔨 Build & Test" as Build
component "🔒 Security Scan" as Security
component "📦 Container Build" as Package
}

database "Registry" as Registry {
component "📋 Azure Container Registry" as ACR
}

cloud "Deployment Stages" as DeploymentStages {
component "🧪 Development" as Dev
component "🎭 Staging" as Staging
component "🚀 Production" as Prod
}

package "Monitoring" as Monitoring {
component "📊 Azure Monitor" as Monitor
component "🚨 Alerting" as Alerts
}

' Pipeline flow
GitHub --> Build : Source Code
Build --> Security : Artifacts
Security --> Package : Verified Code
Package --> ACR : Container Images
ACR --> Dev : Deploy
Dev --> Staging : Promote
Staging --> Prod : Release
Prod --> Monitor : Telemetry
Monitor --> Alerts : Notifications

@enduml

## 7.8 Disaster Recovery

### Backup Strategy

| Component               | Backup Method            | Retention     | Recovery Time |
| ----------------------- | ------------------------ | ------------- | ------------- |
| **PostgreSQL**          | Automated daily backups  | 30 days       | < 1 hour      |
| **Redis**               | Data persistence enabled | Real-time     | < 5 minutes   |
| **Azure Table Storage** | Automatic backups        | 30 days       | < 30 minutes  |
| **Application Code**    | Container registry       | Version-based | < 10 minutes  |
| **Configuration**       | Key Vault backup         | 90 days       | < 5 minutes   |

### Recovery Procedures

1. **Database Recovery**: Point-in-time restore from automated backups
2. **Application Recovery**: Container redeployment from registry
3. **Configuration Recovery**: Key Vault restore and service restart
4. **Network Recovery**: Infrastructure as Code redeployment
