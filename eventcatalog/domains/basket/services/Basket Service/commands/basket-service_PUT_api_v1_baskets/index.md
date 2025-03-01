---
id: basket-service_PUT_api_v1_baskets
version: 1.0.0
name: Update Basket
summary: Update a basket
schemaPath: request-body.json
badges:
  - content: PUT
    textColor: orange
    backgroundColor: orange
  - content: "Basket"
    textColor: yellow
    backgroundColor: yellow
owners:
  - nhanxnguyen
---

## Overview

This endpoint updates an existing basket entity or creates a new one if it doesn't exist. The basket represents an aggregate root in our domain model that maintains its own consistency boundaries.

The operation follows the Repository Pattern for persistence and triggers domain events when the basket state changes. This aligns with our DDD approach by:

1. Treating the basket as a complete aggregate with items as value objects
2. Maintaining invariants during the update operation
3. Encapsulating business rules within the domain model

When a basket is updated, the system publishes integration events that other bounded contexts (like Catalog and Ordering) can subscribe to if needed.

## Architecture

<NodeGraph />

## PUT `(/api/v1/baskets)`

### Request Body

<SchemaViewer file="request-body.json" maxHeight="500" id="request-body" />

### Payload Example

```json title="Payload example"
{
  "id": "b1e1b3b4-1b1b-4b1b-9b1b-1b1b1b1b1b1b",
  "quantity": 1
}
```

### Responses

#### <span className="text-orange-500">400 Bad Request</span>

<SchemaViewer file="response-400.json" maxHeight="500" id="response-400" />

#### <span className="text-green-500">204 No Content</span>
