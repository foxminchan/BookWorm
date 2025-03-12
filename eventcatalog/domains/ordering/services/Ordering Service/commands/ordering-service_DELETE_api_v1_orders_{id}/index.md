---
id: "ordering-service_DELETE_api_v1_orders_{id}"
version: 1.0.0
name: Delete Order
summary: Delete order by order id if exists
schemaPath: ""
badges:
  - content: DELETE
    textColor: red
    backgroundColor: red
  - content: "Order"
    textColor: yellow
    backgroundColor: yellow
owners:
  - nhanxnguyen
---

## Overview

This endpoint allows authorized users to delete an order from the system. In alignment with Domain-Driven Design principles, order deletion is handled as a domain operation that respects the Order aggregate boundary.

### Domain Considerations

- **Deletion Constraints**: Orders can only be deleted if they are in certain states (typically only in draft/pending status)
- **Aggregate Consistency**: The deletion operation ensures the entire Order aggregate (including order lines and related entities) is removed atomically

### Business Rules

- Orders that have already begun processing cannot be deleted, only canceled
- Only orders owned by the requesting user or by administrators can be deleted
- Deletion is a soft-delete operation that maintains data integrity for reporting purposes

### Cross-Domain Effects

When an order is deleted:

- Inventory items that were reserved may be released back to the Catalog service (via domain events)
- The Basket service may be notified if the order originated from a saved basket

## Architecture

<NodeGraph />

## DELETE `(/api/v1/orders/{id})`

### Parameters

- **id** (path) (required): The order id

### Responses

#### <span className="text-green-500">200 OK</span>

<SchemaViewer file="response-200.json" maxHeight="500" id="response-200" />
