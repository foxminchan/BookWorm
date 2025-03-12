---
id: "catalog-service_PUT_api_v1_books_{id}"
version: 1.0.0
name: Update Book
summary: Update a book
schemaPath: request-body.json
badges:
  - content: PUT
    textColor: orange
    backgroundColor: orange
  - content: "Book"
    textColor: yellow
    backgroundColor: yellow
owners:
  - nhanxnguyen
---

## Overview

This endpoint enables the modification of an existing book entity in the catalog domain. Following Domain-Driven Design principles, it updates the aggregate root (Book) while maintaining its invariants and business rules. The operation is idempotent and will return appropriate error responses if the book ID does not exist or if business rules are violated. Updates will be propagated through domain events to maintain consistency across the system.

This endpoint respects the domain boundaries of the Catalog service and enables the modification of book attributes while preserving the entity's identity and integrity.

## Architecture

<NodeGraph />

## PUT `(/api/v1/books/{id})`

### Parameters

- **id** (path) (required)

### Request Body

<SchemaViewer file="request-body.json" maxHeight="500" id="request-body" />

### Responses

#### <span className="text-orange-500">400 Bad Request</span>

<SchemaViewer file="response-400.json" maxHeight="500" id="response-400" />

#### <span className="text-green-500">204 No Content</span>
