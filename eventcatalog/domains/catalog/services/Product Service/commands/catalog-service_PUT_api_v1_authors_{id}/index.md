---
id: "catalog-service_PUT_api_v1_authors_{id}"
version: 1.0.0
name: Update Author
summary: Update an author
schemaPath: request-body.json
badges:
  - content: PUT
    textColor: orange
    backgroundColor: orange
  - content: "Author"
    textColor: yellow
    backgroundColor: yellow
owners:
  - nhanxnguyen
---

## Overview

Update an existing author in the catalog domain. This endpoint adheres to Domain-Driven Design principles by:

- Enforcing domain rules and invariants during update operations
- Validating author data against the bounded context constraints
- Generating domain events upon successful updates
- Maintaining aggregate consistency

The operation is idempotent and will return appropriate error responses if the author ID does not exist or if business rules are violated. Updates will be propagated through domain events to maintain consistency across the system.

## Architecture

<NodeGraph />

## PUT `(/api/v1/authors/{id})`

### Parameters

- **id** (path) (required)

### Request Body

<SchemaViewer file="request-body.json" maxHeight="500" id="request-body" />

### Responses

#### <span className="text-orange-500">400 Bad Request</span>

<SchemaViewer file="response-400.json" maxHeight="500" id="response-400" />

#### <span className="text-green-500">204 No Content</span>
