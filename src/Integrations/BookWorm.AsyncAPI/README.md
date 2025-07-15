# BookWorm.AsyncAPI Integration

## Overview

This integration provides centralized AsyncAPI documentation for all BookWorm services using Saunter and Microsoft.Extensions.ServiceDiscovery. It aggregates AsyncAPI specifications from all discovered services and presents them through a unified interface accessible via the Aspire Dashboard.

## Architecture

### Service Discovery
The integration uses `Microsoft.Extensions.ServiceDiscovery` to automatically discover BookWorm services:
- Catalog Service
- Basket Service  
- Ordering Service
- Rating Service
- Finance Service
- Notification Service
- Chat Service

### AsyncAPI Aggregation
1. **Discovery Phase**: Locates all running BookWorm services using service discovery
2. **Health Check**: Verifies service availability via `/health` endpoints
3. **Specification Retrieval**: Fetches individual AsyncAPI specifications from `/asyncapi/docs/asyncapi.json`
4. **Aggregation**: Merges specifications with service prefixes to avoid naming conflicts
5. **Unified Output**: Provides consolidated AsyncAPI documentation

### Integration Points

#### Aspire Dashboard
- The service is configured in `AppHost.cs` with dependencies on all AsyncAPI-enabled services
- Exposed through the Aspire Dashboard at `/asyncapi/ui` 
- Provides centralized view of all async messaging across the application

#### API Endpoints
- `GET /api/asyncapi/aggregated` - Returns merged AsyncAPI specification
- `GET /api/asyncapi/services` - Lists discovered services and their availability

## Configuration

### Service Dependencies
The AsyncAPI service is configured to:
- Wait for all other services to be ready before starting
- Reference all services that provide AsyncAPI specifications
- Use service discovery to dynamically locate endpoints

### Health Checks
- Integrated with standard BookWorm health check patterns
- Available at `/health` endpoint
- Monitored by HealthChecks UI

## Benefits

1. **Centralized Documentation**: Single point of access for all AsyncAPI specifications
2. **Dynamic Discovery**: Automatically includes new services with AsyncAPI
3. **Service Isolation**: Maintains separation by prefixing schemas and channels
4. **Aspire Integration**: Seamlessly integrated with existing Aspire Dashboard
5. **Health Monitoring**: Real-time status of AsyncAPI-enabled services

## Usage

Once deployed, developers can:
1. Access centralized AsyncAPI documentation through the Aspire Dashboard
2. View real-time service discovery status
3. Navigate individual service specifications within the unified interface
4. Monitor AsyncAPI service health through standard health checks

## Implementation Details

### Service Prefix Strategy
To avoid naming conflicts when merging AsyncAPI specifications:
- Channels are prefixed with service name (e.g., `catalog.book-created`)
- Schemas are prefixed with service name (e.g., `catalog.BookCreatedEvent`)
- Messages are prefixed with service name (e.g., `catalog.BookCreatedMessage`)

### Error Handling
- Graceful degradation when services are unavailable
- Timeout handling for service discovery and specification retrieval
- Comprehensive logging for troubleshooting

### Performance Considerations
- Cached specifications with configurable refresh intervals
- Parallel service discovery and specification retrieval
- Efficient JSON merging algorithms