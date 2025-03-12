---
id: ordering-service_GET_api_v1_buyers_me
version: 1.0.0
name: Get Buyer
summary: Get current buyer
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

This endpoint retrieves the current buyer's information from the Ordering bounded context. In our domain model, a Buyer represents a customer entity with purchasing capabilities and order history.

### Domain Significance

Within the Ordering domain, the Buyer aggregate is responsible for:

- Maintaining buyer identity and preferences
- Tracking order history and purchasing patterns
- Managing payment methods and shipping addresses

### Implementation Details

This endpoint follows the CQRS pattern using a dedicated query handler that:

1. Authenticates the current user
2. Retrieves the corresponding Buyer aggregate from the repository
3. Returns a read-model projection of the buyer data

### Administrative Access

Administrators can access specific buyer information by providing an optional query parameter:

- `?id={buyerId}`: Retrieve a specific buyer by their unique identifier (admin only)

When accessed by regular users, the endpoint always returns the buyer information associated with the authenticated user's identity.

## Architecture

<NodeGraph />

## GET `(/api/v1/buyers/me)`

### Parameters

- **id** (path) (required): The order id

### Responses

#### <span className="text-green-500">200 OK</span>

<SchemaViewer file="response-200.json" maxHeight="500" id="response-200" />
