---
id: catalog-service_POST_api_v1_books
version: 1.0.0
name: Create Book
summary: Create a book
schemaPath: request-body.json
badges:
  - content: POST
    textColor: green
    backgroundColor: green
  - content: "Book"
    textColor: yellow
    backgroundColor: yellow
owners:
  - nhanxnguyen
---

## Overview

The Create Book endpoint allows administrators to add new books to the Catalog domain. This operation follows Domain-Driven Design principles by:

1. Validating the book entity against domain rules and invariants
2. Creating a new aggregate root in the Books collection
3. Publishing a `BookCreatedEvent` that other bounded contexts can subscribe to

This endpoint represents the command side of our CQRS pattern implementation. The book entity becomes immediately available for queries after successful creation.

:::note
That book creation requires proper authentication with Author privileges, as indicated by the badge.
:::

## Architecture

<NodeGraph />

## POST `(/api/v1/books)`

### Parameters

- **id** (path) (required)

### Request Body

<SchemaViewer file="request-body.json" maxHeight="500" id="request-body" />

### Responses

#### <span className="text-orange-500">400 Bad Request</span>

<SchemaViewer file="response-400.json" maxHeight="500" id="response-400" />

#### <span className="text-green-500">201 Created</span>
