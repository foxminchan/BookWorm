---
id: catalog-service_POST_api_v1_categories
version: 1.0.0
name: Create Category
summary: Create a category
schemaPath: request-body.json
badges:
  - content: POST
    textColor: green
    backgroundColor: green
  - content: "Category"
    textColor: yellow
    backgroundColor: yellow
owners:
  - nhanxnguyen
---

## Overview

Creates a new product category in the catalog domain. Categories are aggregates within our domain model that organize products into hierarchical structures.

This endpoint follows Domain-Driven Design principles by enforcing business rules at the domain layer, ensuring category names are unique and properly formatted. It uses the Command pattern through a `CreateCategoryCommand` object which is validated before being processed by the domain service.

## Architecture

<NodeGraph />

## POST `(/api/v1/categories)`

### Parameters

- **id** (path) (required)

### Request Body

<SchemaViewer file="request-body.json" maxHeight="500" id="request-body" />

### Responses

#### <span className="text-orange-500">400 Bad Request</span>

<SchemaViewer file="response-400.json" maxHeight="500" id="response-400" />

#### <span className="text-green-500">201 Created</span>
