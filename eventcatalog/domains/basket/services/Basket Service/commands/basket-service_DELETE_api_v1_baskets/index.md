---
id: basket-service_DELETE_api_v1_baskets
version: 1.0.0
name: Delete Basket
summary: Delete a basket by its unique identifier
schemaPath: request-body.json
badges:
  - content: DELETE
    textColor: red
    backgroundColor: red
  - content: "Basket"
    textColor: yellow
    backgroundColor: yellow
owners:
  - nhanxnguyen
---

## Overview

This endpoint allows you to delete a shopping basket by its unique identifier. This operation is an essential part of the Basket domain's lifecycle management.

### Business Rules

- Basket must exist to be deleted
- Only the owner of the basket or an administrator can delete a basket
- Deleting a basket will permanently remove all items within it

### Implementation Details

The delete operation is handled by the `DeleteBasketCommandHandler` which enforces the domain rules and publishes appropriate events through our event bus to maintain system consistency.

## Architecture

<NodeGraph />

## DELETE `(/api/v1/baskets)`

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
