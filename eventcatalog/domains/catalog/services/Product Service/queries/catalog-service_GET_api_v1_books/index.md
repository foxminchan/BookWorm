---
id: catalog-service_GET_api_v1_books
version: 1.0.0
name: List Books
summary: Get all books
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

Get all books in the system with pagination support. This endpoint retrieves a collection of book aggregates from the Catalog bounded context, applying domain filters and returning DTOs that comply with our anti-corruption layer patterns.

The endpoint respects the read model separation in our CQRS implementation, using dedicated read models optimized for query performance. Results can be filtered by various domain attributes including genre, author, and publication status.

Each book in the returned collection contains core domain properties while maintaining a clear separation between entity identity and value objects according to DDD principles.

## Architecture

<NodeGraph />

## GET `(/api/v1/books)`

### Parameters

- **id** (path) (required)

### Request Body

<SchemaViewer file="request-body.json" maxHeight="500" id="request-body" />

### Responses

#### <span className="text-orange-500">400 Bad Request</span>

<SchemaViewer file="response-400.json" maxHeight="500" id="response-400" />

#### <span className="text-green-500">200 OK</span>
