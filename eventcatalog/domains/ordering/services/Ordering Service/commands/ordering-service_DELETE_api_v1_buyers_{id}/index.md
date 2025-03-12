---
id: "ordering-service_DELETE_api_v1_buyers_{id}"
version: 1.0.0
name: Delete Buyer
summary: Delete buyer by buyer id if exists
schemaPath: ""
badges:
  - content: DELETE
    textColor: red
    backgroundColor: red
  - content: "Buyer"
    textColor: yellow
    backgroundColor: yellow
owners:
  - nhanxnguyen
---

## Overview

This endpoint allows for the removal of a Buyer aggregate from the system. In the Ordering domain, a Buyer represents a customer who has placed one or more orders.

## Architecture

<NodeGraph />

### Domain Significance

Within the bounded context of the Ordering domain, the Buyer aggregate root contains important customer information related to ordering, including:

- Address information
- Order history references

### Command Handling

When this endpoint is called:

1. A `DeleteBuyerCommand` is dispatched via the CQRS pattern
2. The command handler retrieves the Buyer aggregate
3. Domain validations ensure the Buyer can be safely deleted (e.g., no pending orders)
4. If valid, the Buyer is marked for deletion in the domain
5. The repository persists this change

### Consistency Considerations

This operation maintains aggregate consistency by ensuring all entities within the Buyer aggregate boundary are properly removed. The operation is atomic - either the entire Buyer aggregate is deleted or none of it is.

### Business Rules

- A Buyer cannot be deleted if they have pending orders
- Associated payment methods are invalidated as part of the deletion
- Historical order data may be preserved depending on retention policies

## DELETE `(/api/v1/buyers/{id})`

### Parameters

- **id** (path) (required): The order id

### Responses

#### <span className="text-green-500">200 OK</span>

<SchemaViewer file="response-200.json" maxHeight="500" id="response-200" />
