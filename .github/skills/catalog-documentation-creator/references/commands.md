# Commands

## Format

**File:** `index.mdx` inside a command folder
**Location:** `commands/{CommandName}/index.mdx` or nested under a service `domains/{Domain}/services/{Service}/commands/{CommandName}/index.mdx`

## Frontmatter Fields

| Field        | Required | Description                                                         |
| ------------ | -------- | ------------------------------------------------------------------- |
| `id`         | Yes      | Unique identifier, typically PascalCase (e.g., `AddInventory`)      |
| `name`       | Yes      | Human-readable name                                                 |
| `version`    | Yes      | Semver string (e.g., `0.0.1`)                                       |
| `summary`    | Yes      | 1-2 sentence description of what this command does                  |
| `owners`     | Yes      | Array of team or user IDs                                           |
| `schemaPath` | No       | Path to schema file (e.g., `schema.json`)                           |
| `badges`     | No       | Array of badge objects                                              |
| `operation`  | No       | Object with `method`, `path`, and `statusCodes` for REST operations |

## Example 1: Command with REST Operation

````mdx
---
id: AddInventory
name: Add inventory
version: 0.0.3
summary: |
  Command that will add item to a given inventory id
owners:
  - dboyne
  - full-stack
badges:
  - content: Recently updated!
    backgroundColor: green
    textColor: green
schemaPath: schema.json
operation:
  method: POST
  path: /inventory
  statusCodes: ["201", "400"]
---

## Overview

The AddInventory command is issued to add new stock to the inventory. This command is used by the inventory management system to update the quantity of products available in the warehouse or store.

## Architecture diagram

<NodeGraph />

## Payload example

```json
{
  "productId": "789e1234-b56c-78d9-e012-3456789fghij",
  "quantity": 50,
  "warehouseId": "456e7891-c23d-45f6-b78a-123456789abc",
  "timestamp": "2024-07-04T14:48:00Z"
}
```
````

## Schema

<Schema file="schema.json"/>
```

## Example 2: Simple Command

````mdx
---
id: CancelShipment
name: Cancel Shipment
version: 0.0.1
summary: |
  Command to cancel a pending shipment before it has been dispatched.
owners:
  - shipping-team
schemaPath: schema.json
operation:
  method: DELETE
  path: /shipments/{shipmentId}
  statusCodes: ["200", "404", "409"]
---

## Overview

The CancelShipment command cancels a shipment that has not yet been dispatched. Returns 409 Conflict if the shipment is already in transit.

<NodeGraph />

## Payload example

```json
{
  "shipmentId": "ship-12345",
  "reason": "customer_cancelled_order",
  "cancelledBy": "system"
}
```
````

## Schema

<SchemaViewer file="schema.json" title="JSON Schema" maxHeight="500" />
```

## Key Conventions

- Commands represent requests for action (writes/mutations)
- Use `operation` field for REST-style commands to document method, path, and status codes
- Include `<NodeGraph />` to show which services handle this command
