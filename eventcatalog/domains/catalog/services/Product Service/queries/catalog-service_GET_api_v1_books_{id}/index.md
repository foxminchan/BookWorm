---
id: "catalog-service_GET_api_v1_books_{id}"
version: 1.0.0
name: Get Book
summary: Get a book by ID
schemaPath: request-body.json
badges:
  - content: GET
    textColor: blue
    backgroundColor: blue
  - content: "Book"
    textColor: yellow
    backgroundColor: yellow
owners:
  - nhanxnguyen
---

## Overview

This endpoint follows Domain-Driven Design principles to retrieve a specific Book aggregate root from the Catalog bounded context by its unique identifier. The operation is implemented as a query that doesn't modify state, adhering to CQRS patterns.

The query handler maps the domain entity to a BookDto response through an auto-mapper profile, ensuring that domain implementation details remain encapsulated. The endpoint respects the aggregate boundaries and only exposes data appropriate for the presentation layer.

If the requested book doesn't exist, the service returns a Not Found response rather than a null object, following the Fail Fast principle.

This endpoint is also integrated with our distributed caching strategy to optimize read-heavy operations and reduce database load.

## Architecture

<NodeGraph />
## GET `(/api/v1/books/{id})`

### Parameters

- **id** (path) (required)

### Request Body

<SchemaViewer file="request-body.json" maxHeight="500" id="request-body" />

### Responses

#### <span className="text-orange-500">400 Bad Request</span>

<SchemaViewer file="response-400.json" maxHeight="500" id="response-400" />

#### <span className="text-green-500">200 OK</span>
