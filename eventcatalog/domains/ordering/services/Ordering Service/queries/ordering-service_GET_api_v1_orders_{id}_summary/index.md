---
id: "ordering-service_GET_api_v1_orders_{id}_summary"
version: 1.0.0
name: Get Order Summary
summary: Get order summary from event store
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

This endpoint retrieves the current state of an Order aggregate by replaying events from the event store, following our event sourcing pattern within the Ordering bounded context.

The Order Summary represents a read model derived from domain events that captures the essential information about an order's current state, including:

- Order identification and metadata
- Pricing information
- Delivery information and status

This endpoint demonstrates the CQRS (Command Query Responsibility Segregation) principle by providing a dedicated read model optimized for client consumption. The Order aggregate rebuilds its state by sequentially applying all recorded domain events associated with the specified order ID.

## Architecture

<NodeGraph />

## GET `(/api/v1/orders/{id}/summary)`

### Parameters

- **id** (path) (required): The order id

### Responses

#### <span className="text-green-500">200 OK</span>

<SchemaViewer file="response-200.json" maxHeight="500" id="response-200" />
