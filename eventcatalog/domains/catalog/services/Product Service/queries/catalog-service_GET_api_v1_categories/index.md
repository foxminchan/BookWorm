---
id: catalog-service_GET_api_v1_categories
version: 1.0.0
name: List Categories
summary: Get all categories
schemaPath: request-body.json
badges:
  - content: GET
    textColor: blue
    backgroundColor: blue
  - content: "Category"
    textColor: yellow
    backgroundColor: yellow
owners:
  - nhanxnguyen
---

## Overview

Get all categories in the system. This query endpoint retrieves the complete collection of categories from the Catalog bounded context without filtering. Categories represent a core domain concept within the catalog taxonomy and are used for organizing book inventory.

The endpoint follows the CQRS pattern, implementing a read-only query that returns a denormalized view of category data optimized for client consumption. Categories are aggregates within the catalog domain model and this endpoint preserves their encapsulation by returning only the necessary projection data.

This operation is idempotent and cacheable as it performs no state mutations.

## Architecture

<NodeGraph />

## GET `(/api/v1/categories)`

### Parameters

- **id** (path) (required)

### Request Body

<SchemaViewer file="request-body.json" maxHeight="500" id="request-body" />

### Responses

#### <span className="text-orange-500">400 Bad Request</span>

<SchemaViewer file="response-400.json" maxHeight="500" id="response-400" />

#### <span className="text-green-500">200 OK</span>
