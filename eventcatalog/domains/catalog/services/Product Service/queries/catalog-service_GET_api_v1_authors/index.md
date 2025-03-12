---
id: catalog-service_GET_api_v1_authors
version: 1.0.0
name: List Authors
summary: Get all authors
schemaPath: request-body.json
badges:
  - content: GET
    textColor: blue
    backgroundColor: blue
  - content: "Author"
    textColor: yellow
    backgroundColor: yellow
owners:
  - nhanxnguyen
---

## Overview

This endpoint retrieves a comprehensive list of all authors available in the catalog domain. The implementation follows Domain-Driven Design principles, separating the concern of querying authors from the domain model through a dedicated query handler.

The query accesses the repository abstraction rather than directly querying the database, maintaining proper encapsulation of the persistence layer. The returned authors are mapped to DTOs using AutoMapper, ensuring that domain entities remain protected from external exposure.

This endpoint supports catalog bounded context operations that require author information, such as book browsing and filtering. It provides a paginated response to handle potentially large datasets efficiently.

## Architecture

<NodeGraph />

## GET `(/api/v1/authors)`

### Parameters

- **id** (path) (required)

### Request Body

<SchemaViewer file="request-body.json" maxHeight="500" id="request-body" />

### Responses

#### <span className="text-orange-500">400 Bad Request</span>

<SchemaViewer file="response-400.json" maxHeight="500" id="response-400" />

#### <span className="text-green-500">200 OK</span>
