---
id: catalog-service_POST_api_v1_authors
version: 1.0.0
name: Create Author
summary: Create an author
schemaPath: request-body.json
badges:
  - content: POST
    textColor: green
    backgroundColor: green
  - content: "Author"
    textColor: yellow
    backgroundColor: yellow
owners:
  - nhanxnguyen
---

## Overview

This endpoint implements the command part of CQRS pattern to create a new Author entity in the Catalog bounded context. It follows Domain-Driven Design principles by encapsulating the author creation operation as a discrete business capability.

Authorization is enforced through policy-based permissions, ensuring only authenticated users with appropriate roles can create authors in the system.

Performance considerations:

- Author creation is idempotent
- Validation occurs before persistence
- Transactional boundaries are respected

## Architecture

<NodeGraph />

## POST `(/api/v1/authors)`

### Parameters

- **id** (path) (required)

### Request Body

<SchemaViewer file="request-body.json" maxHeight="500" id="request-body" />

### Responses

#### <span className="text-orange-500">400 Bad Request</span>

<SchemaViewer file="response-400.json" maxHeight="500" id="response-400" />

#### <span className="text-green-500">201 Created</span>
