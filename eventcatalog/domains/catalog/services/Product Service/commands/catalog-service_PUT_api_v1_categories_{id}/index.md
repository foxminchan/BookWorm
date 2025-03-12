---
id: "catalog-service_PUT_api_v1_categories_{id}"
version: 1.0.0
name: Update Category
summary: Update a category
schemaPath: request-body.json
badges:
  - content: PUT
    textColor: orange
    backgroundColor: orange
  - content: "Category"
    textColor: yellow
    backgroundColor: yellow
owners:
  - nhanxnguyen
---

## Overview

This endpoint allows modification of an existing category entity within the Catalog domain. It follows Domain-Driven Design principles by ensuring that the category update operation maintains domain integrity and business rules.

The operation performs the following:

1. Validates the incoming category update command against domain invariants
2. Retrieves the existing category aggregate from the repository
3. Updates the category properties while preserving aggregate consistency
4. Persists changes through the repository pattern

Note that this endpoint enforces proper domain authorization and validation rules. The operation is transactional and will either succeed completely or fail without partial updates.

## Architecture

<NodeGraph />

## PUT `(/api/v1/categories/{id})`

### Parameters

- **id** (path) (required)

### Request Body

<SchemaViewer file="request-body.json" maxHeight="500" id="request-body" />

### Responses

#### <span className="text-orange-500">400 Bad Request</span>

<SchemaViewer file="response-400.json" maxHeight="500" id="response-400" />

#### <span className="text-green-500">204 No Content</span>
