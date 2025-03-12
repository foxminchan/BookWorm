---
id: Notification Service
name: Notification Service
version: 1.0.0
summary: Notification Worker for BookWorm
badges: []
sends: []
receives:
  - id: cancelordercommand.message
    version: 1.0.0
  - id: completeordercommand.message
    version: 1.0.0
  - id: placeordercommand.message
    version: 1.0.0
schemaPath: notification-service.yml
specifications:
  asyncapiPath: notification-service.yml
owners:
  - nhanxnguyen
repository:
  language: C#
  url: https://github.com/foxminchan/BookWorm
---

## Overview

The Notification Service is a core domain service within BookWorm's microservices architecture, responsible for managing all communication with users across the platform. Built following Domain-Driven Design principles, this service maintains a clear bounded context focused on notification delivery and management.

### Domain Model

The service is organized around the following aggregates:

- **NotificationAggregate**: The root entity managing notification lifecycle
- **Template**: Value objects representing message templates
- **DeliveryChannel**: Entities for email, push, and SMS delivery mechanisms

### Event-Driven Architecture

The Notification Service consumes domain events from other bounded contexts to trigger appropriate communications:

- Order placement confirmations
- Order cancellation notices
- Order completion notifications

### Capabilities

- Template-based notification generation
- Delivery tracking and confirmation
- User notification preferences management

### Technical Implementation

Implemented as a background worker service that processes notification requests asynchronously, ensuring reliable delivery while maintaining system performance under load.

## Architecture diagram

<NodeGraph />
