---
id: catalog-service_GET_api_v1_publishers
version: 1.0.0
name: List Publishers
summary: Get all publishers
schemaPath: request-body.json
badges:
  - content: GET
    textColor: blue
    backgroundColor: blue
  - content: "Publisher"
    textColor: blue
    backgroundColor: blue
owners:
  - nhanxnguyen
---

## Overview

Get all publishers in the system. This endpoint retrieves a comprehensive list of book publishers from our catalog domain. Publishers represent important aggregate roots in our domain model that have relationships with multiple book entities. This query operation implements the Repository pattern to efficiently fetch publisher data while maintaining domain boundaries.

The operation follows CQRS principles by using a dedicated read model query handler that optimizes for performance when retrieving this frequently accessed reference data. The response includes essential publisher metadata while maintaining proper encapsulation of internal domain concepts.

## Architecture

<NodeGraph />

## GET `(/api/v1/publishers)`

### Parameters

- **id** (path) (required)

### Request Body

<SchemaViewer file="request-body.json" maxHeight="500" id="request-body" />

### Responses

#### <span className="text-orange-500">400 Bad Request</span>

<SchemaViewer file="response-400.json" maxHeight="500" id="response-400" />

#### <span className="text-green-500">200 OK</span>
