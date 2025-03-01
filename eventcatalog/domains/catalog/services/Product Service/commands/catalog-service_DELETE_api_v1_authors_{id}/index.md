---
id: "catalog-service_DELETE_api_v1_authors_{id}"
version: 1.0.0
name: Delete Author
summary: Delete an author
schemaPath: request-body.json
badges:
  - content: DELETE
    textColor: red
    backgroundColor: red
  - content: "Author"
    textColor: yellow
    backgroundColor: yellow
owners:
  - nhanxnguyen
---

## Overview

This endpoint allows for the removal of an Author entity from the Catalog domain. Following Domain-Driven Design principles, this operation:

- Respects the Author aggregate boundary
- Enforces domain invariants and business rules
- Validates that the author can be safely removed without violating referential integrity
- Raises appropriate domain events (AuthorDeletedDomainEvent) when successful
- Returns proper error responses if the operation violates any domain constraints

The implementation uses a CQRS pattern with a DeleteAuthorCommand processed by MediatR, maintaining separation between the command interface and the domain logic.

:::note
Authors with associated books cannot be deleted until those relationships are removed first, preserving domain consistency.
:::

## Architecture

<NodeGraph />

## DELETE `(/api/v1/authors/{id})`

### Parameters

- **id** (path) (required)

### Request Body

<SchemaViewer file="request-body.json" maxHeight="500" id="request-body" />

### Payload Example

```json title="Payload example"
{
  "id": "a1e1b3b4-1b1b-4b1b-9b1b-1b1b1b1b1b1b",
  "name": "John Doe"
}
```

### Responses

#### <span className="text-orange-500">400 Bad Request</span>

<SchemaViewer file="response-400.json" maxHeight="500" id="response-400" />

#### <span className="text-green-500">204 No Content</span>
