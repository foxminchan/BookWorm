---
id: "ordering-service_PATCH_api_v1_orders_{orderId}_cancel"
version: 1.0.0
name: Cancel Order
summary: Cancel order by order id
schemaPath: ""
badges:
  - content: PATCH
    textColor: orange
    backgroundColor: orange
  - content: "Order"
    textColor: yellow
    backgroundColor: yellow
owners:
  - nhanxnguyen
---

## Overview

The Order Cancellation operation is a critical boundary-crossing interaction within the Ordering domain. This endpoint empowers users to cancel an existing order that hasn't progressed beyond a cancellable state in its lifecycle.

From a domain perspective, order cancellation represents an important state transition that triggers several domain events and side effects:

1. **State Validation**: The domain enforces business rules to verify if the order is eligible for cancellation based on its current state (typically allowed only in "Pending", "Processing" or other early states).

2. **Domain Event Publication**: Upon successful cancellation, the `OrderCancelledDomainEvent` is raised, which may trigger compensating transactions across bounded contexts.

3. **Aggregate Consistency**: The Order aggregate's invariants are preserved throughout the cancellation process, ensuring the order transitions to a cancelled state only when business rules permit.

4. **Inventory Impact**: Cancelled orders may trigger inventory replenishment commands to the Catalog service via integration events.

5. **Notification Generation**: The cancellation typically initiates notification events to inform customers about their order status change.

This operation adheres to our domain-centric design principles, encapsulating business rules within the Order aggregate while exposing a simple HTTP interface for clients.

## Architecture

<NodeGraph />

## PATCH `(/api/v1/orders/{orderId}/cancel)`

### Parameters

- **id** (path) (required): The order id

### Responses

#### <span className="text-green-500">200 OK</span>

<SchemaViewer file="response-200.json" maxHeight="500" id="response-200" />
