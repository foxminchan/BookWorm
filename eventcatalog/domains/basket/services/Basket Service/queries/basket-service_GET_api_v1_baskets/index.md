---
id: basket-service_GET_api_v1_baskets
version: 1.0.0
name: Get Basket
summary: Get a basket by user
schemaPath: request-body.json
badges:
  - content: GET
    textColor: blue
    backgroundColor: blue
  - content: "Basket"
    textColor: yellow
    backgroundColor: yellow
owners:
  - nhanxnguyen
---

## Overview

This endpoint retrieves a user's shopping basket based on their identity. In our domain model, a Basket is an aggregate root that contains a collection of BasketItems, each representing a book the user intends to purchase.

### Domain Context

Within our bounded context, the basket represents the current selection of items a user has chosen but not yet purchased. The basket is identified by a unique user identifier and maintains the state of the user's shopping session.

### Business Rules

- Each user can have only one active basket
- Basket items contain references to catalog items (books) with quantity
- Prices are stored in the basket to maintain price consistency during the shopping session
- Anonymous users' baskets are tracked via temporary identifiers

### Use Cases

- Initial page load for returning users
- Checkout process initiation
- Basket summary display

### Integration Points

This endpoint is consumed by the web UI and integrates with the Catalog service to fetch current book information.

## Architecture

<NodeGraph />

## GET `(/api/v1/baskets)`

### Request Body

<SchemaViewer file="request-body.json" maxHeight="500" id="request-body" />

### Responses

#### <span className="text-orange-500">400 Bad Request</span>

<SchemaViewer file="response-400.json" maxHeight="500" id="response-400" />

#### <span className="text-green-500">200 OK</span>
