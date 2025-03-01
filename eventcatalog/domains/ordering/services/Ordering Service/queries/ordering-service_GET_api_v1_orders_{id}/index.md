---
id: "ordering-service_GET_api_v1_orders_{id}"
version: 1.0.0
name: Get Order
summary: Get order detail by order id
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

The `Get Order` endpoint provides access to a complete Order aggregate within the Ordering bounded context. This read operation aligns with the Query side of our CQRS pattern implementation, retrieving a fully hydrated Order aggregate root along with its associated value objects and entities.

Within our domain model, an Order represents a crucial business document that encapsulates the entire purchasing transaction. The Order aggregate maintains several important invariants and contains:

- Order header information with customer identity and delivery details
- Collection of OrderItem entities representing the purchased products
- Order status reflecting its position in the fulfillment lifecycle

This endpoint serves various domain use cases including order tracking, fulfillment processing, and customer service inquiries. It enforces access control to ensure that only authorized contexts can retrieve order information.

As per DDD principles, the internal domain model is not exposed directly - instead, a DTO representation is returned that contains all necessary information while preserving the integrity of our domain boundaries.

## Architecture

<NodeGraph />

## GET `(/api/v1/orders/{id})`

### Parameters

- **id** (path) (required): The order id

### Responses

#### <span className="text-green-500">200 OK</span>

<SchemaViewer file="response-200.json" maxHeight="500" id="response-200" />
