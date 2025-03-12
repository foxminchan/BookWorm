---
id: basket-service_POST_api_v1_baskets
version: 1.0.0
name: Create Basket
summary: Create a new basket for a user
schemaPath: request-body.json
badges:
  - content: POST
    textColor: green
    backgroundColor: green
  - content: "Basket"
    textColor: yellow
    backgroundColor: yellow
owners:
  - nhanxnguyen
---

## Overview

This endpoint creates a new shopping basket for a user. In our domain model, a `Basket` represents an Aggregate Root in the Basket bounded context. Each Basket maintains its own identity and encapsulates a collection of `BasketItem` entities, which are part of the Basket's aggregate boundary.

The `Basket` enforces invariants across its contained items and manages the lifecycle of the `BasketItem` entities through well-defined domain operations. When a basket is created, it establishes the consistency boundary for transactional operations related to the user's shopping experience.

Each `BasketItem` captures the user's intent to purchase a specific book, maintaining quantity and price information while preserving the connection to the Catalog domain through book identifiers.

## Architecture

<NodeGraph />

## POST `(/api/v1/baskets)`

### Request Body

<SchemaViewer file="request-body.json" maxHeight="500" id="request-body" />

### Responses

#### <span className="text-orange-500">400 Bad Request</span>

<SchemaViewer file="response-400.json" maxHeight="500" id="response-400" />

#### <span className="text-green-500">201 Created</span>
