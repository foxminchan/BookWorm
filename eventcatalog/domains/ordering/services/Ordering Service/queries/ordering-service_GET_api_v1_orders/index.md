---
id: ordering-service_GET_api_v1_orders
version: 1.0.0
name: List Orders
summary: Get orders in a paged format
schemaPath: ""
badges:
  - content: GET
    textColor: green
    backgroundColor: green
  - content: "Order"
    textColor: yellow
    backgroundColor: yellow
owners:
  - nhanxnguyen
---

## Overview

The `GET /api/v1/orders` endpoint provides access to the Order aggregate roots within the Ordering bounded context. This endpoint implements a paginated collection pattern to efficiently retrieve and present Order entities while maintaining system performance under load.

### Domain Significance

Orders represent completed purchase intentions by customers and serve as the primary aggregate root in the Ordering domain. Each Order encapsulates:

- Customer information
- Order items with their quantities and prices
- Order status within the fulfillment lifecycle

### Implementation Details

The endpoint uses page-based navigation to handle potentially large datasets according to domain constraints. Pagination parameters follow the standard collection pattern:

- `pageIndex` (default: 0): The zero-based page index
- `pageSize` (default: 10): Number of Order aggregates per page
- `sortBy` (optional): Property name to sort by (e.g., "OrderDate")
- `sortDirection` (optional): "asc" or "desc"

### Domain Projections

The returned data represents a read-model projection of the Order aggregate, optimized for the listing use case while preserving domain invariants. This approach aligns with CQRS principles by separating the read concern from the write models.

### Authorization Context

Access to orders is governed by domain policies that determine which orders a particular user can view based on their identity and roles within the system.

## Architecture

<NodeGraph />

## GET `(/api/v1/orders)`

### Parameters

- **id** (path) (required): The order id

### Responses

#### <span className="text-green-500">200 OK</span>

<SchemaViewer file="response-200.json" maxHeight="500" id="response-200" />
