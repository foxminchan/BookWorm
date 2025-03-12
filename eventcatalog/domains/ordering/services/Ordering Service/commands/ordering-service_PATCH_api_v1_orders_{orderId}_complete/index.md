---
id: "ordering-service_PATCH_api_v1_orders_{orderId}_complete"
version: 1.0.0
name: Complete Order
summary: Complete order by order id
schemaPath: ""
badges:
  - content: PATCH
    textColor: blue
    backgroundColor: blue
  - content: "Order"
    textColor: yellow
    backgroundColor: yellow
owners:
  - nhanxnguyen
---

## Overview

This endpoint marks an order as complete within the Ordering bounded context, representing the final state transition in the order fulfillment lifecycle. When an order is completed, it signifies that all items have been delivered to the customer and the business transaction is finalized.

### Domain Significance

In our domain model, order completion is a crucial state transition that:

- Validates that the order is in a valid state for completion
- Ensures all order items have been fulfilled
- Triggers domain events for cross-service communication
- Updates aggregate state while preserving invariants

### Domain Events

Upon successful completion, the system publishes an `OrderCompletedDomainEvent` that notifies other bounded contexts (particularly Notification and Rating services) about this state change. The Notification service sends a confirmation to the customer, while the Rating service may prompt for product reviews.

### Business Rules

- Only orders in "Shipped" status can be completed
- Orders with unresolved disputes cannot be completed
- Completion timestamp must be recorded for audit purposes
- Customer loyalty points are calculated and awarded at completion

This operation is idempotent and will return the current state if the order is already completed.

## Architecture

<NodeGraph />

## PATCH `(/api/v1/orders/{orderId}/complete)`

### Parameters

- **id** (path) (required): The order id

### Responses

#### <span className="text-green-500">200 OK</span>

<SchemaViewer file="response-200.json" maxHeight="500" id="response-200" />
