---
id: ordering-service_GET_api_v1_buyers
version: 1.0.0
name: List Buyers
summary: Get buyers in a paged format
schemaPath: ""
badges:
  - content: GET
    textColor: green
    backgroundColor: green
  - content: "Buyer"
    textColor: yellow
    backgroundColor: yellow
owners:
  - nhanxnguyen
---

## Overview

Get buyers in a paged format
This endpoint is part of the Buyer aggregate in the Ordering bounded context. In our domain model, a Buyer represents a customer who can place orders within the system.

### Domain Context

Within the Ordering bounded context, Buyers are important aggregate roots that encapsulate order history, payment methods, and delivery preferences. The paged query approach respects the query optimization patterns in DDD by:

- Implementing a read-focused projection of the Buyer aggregate
- Using pagination to maintain performance with potentially large datasets
- Exposing only the relevant Buyer attributes needed for listing scenarios

### Implementation Details

This query is implemented using CQRS pattern:

- **Command/Query Separation**: This read-only endpoint uses a dedicated query handler
- **Materialized View**: Returns a lightweight DTO projection optimized for reads
- **Repository Pattern**: Abstracts the underlying data access concerns

The pagination follows DDD best practices by treating page size and number as domain concepts rather than technical implementation details, allowing business rules to dictate appropriate limits.

## Architecture

<NodeGraph />

## GET `(/api/v1/buyers)`

### Parameters

- **id** (path) (required): The order id

### Responses

#### <span className="text-green-500">200 OK</span>

<SchemaViewer file="response-200.json" maxHeight="500" id="response-200" />
