---
id: "catalog-service_PUT_api_v1_publishers_{id}"
version: 1.0.0
name: Update Publisher
summary: Update a publisher
schemaPath: request-body.json
badges:
  - content: PUT
    textColor: orange
    backgroundColor: orange
  - content: "Publisher"
    textColor: yellow
    backgroundColor: yellow
owners:
  - nhanxnguyen
---

## Overview

Update an existing publisher in the catalog domain. This endpoint follows Domain-Driven Design principles, where the publisher entity is part of the catalog bounded context. When a publisher is updated, the command is validated against business rules before being processed by the domain service.

The operation is idempotent and ensures data integrity through optimistic concurrency control. If the publisher doesn't exist, the system will return an appropriate error response rather than creating a new entity, maintaining the command-query separation principle.

This endpoint supports the catalog domain's business capability to manage publisher information as a core domain concept.

## Architecture

<NodeGraph />

## PUT `(/api/v1/publishers/{id})`

### Parameters

- **id** (path) (required)

### Request Body

<SchemaViewer file="request-body.json" maxHeight="500" id="request-body" />

### Responses

#### <span className="text-orange-500">400 Bad Request</span>

<SchemaViewer file="response-400.json" maxHeight="500" id="response-400" />

#### <span className="text-green-500">204 No Content</span>
