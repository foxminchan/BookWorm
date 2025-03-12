---
id: "catalog-service_DELETE_api_v1_books_{id}"
version: 1.0.0
name: Delete Book
summary: Delete a book if it exists
schemaPath: request-body.json
badges:
  - content: DELETE
    textColor: red
    backgroundColor: red
  - content: "Book"
    textColor: yellow
    backgroundColor: yellow
owners:
  - nhanxnguyen
---

## Overview

The DELETE operation removes a book entity from the catalog domain if it exists. This endpoint enforces proper domain boundaries by validating the book's existence and ensuring there are no constraint violations before deletion.

The operation is idempotent - multiple identical requests will have the same effect as a single request, aligning with REST architectural constraints.

**Authorization**: Requires "ADMIN" role to maintain aggregate integrity.

**Domain Validation**:

- Validates book exists before deletion
- Checks for business rules that might prevent deletion (e.g., books with active orders)

## Architecture

<NodeGraph />

## DELETE `(/api/v1/books/{id})`

### Parameters

- **id** (path) (required)

### Request Body

<SchemaViewer file="request-body.json" maxHeight="500" id="request-body" />

### Responses

#### <span className="text-orange-500">400 Bad Request</span>

<SchemaViewer file="response-400.json" maxHeight="500" id="response-400" />

#### <span className="text-green-500">204 No Content</span>
