# Queries

## Format

**File:** `index.mdx` inside a query folder
**Location:** `queries/{QueryName}/index.mdx` or nested under a service `domains/{Domain}/services/{Service}/queries/{QueryName}/index.mdx`

## Frontmatter Fields

| Field        | Required | Description                                                         |
| ------------ | -------- | ------------------------------------------------------------------- |
| `id`         | Yes      | Unique identifier, typically PascalCase (e.g., `GetOrderStatus`)    |
| `name`       | Yes      | Human-readable name                                                 |
| `version`    | Yes      | Semver string (e.g., `0.0.1`)                                       |
| `summary`    | Yes      | 1-2 sentence description of what this query returns                 |
| `owners`     | Yes      | Array of team or user IDs                                           |
| `schemaPath` | No       | Path to schema file (e.g., `schema.json`)                           |
| `badges`     | No       | Array of badge objects                                              |
| `operation`  | No       | Object with `method`, `path`, and `statusCodes` for REST operations |

## Example: Query with REST Operation and Schema

````mdx
---
id: GetInventoryStatus
name: Get inventory status
version: 0.0.1
summary: |
  GET request that will return the current stock status for a specific product.
owners:
  - dboyne
badges:
  - content: GET Request
    backgroundColor: green
    textColor: green
schemaPath: schema.json
operation:
  method: GET
  path: /inventory/{productId}/status
  statusCodes: ["200", "404"]
---

## Overview

The GetInventoryStatus message is a query designed to retrieve the current stock status for a specific product.

This query provides detailed information about the available quantity, reserved quantity, and the warehouse location where the product is stored. It is typically used by systems or services that need to determine the real-time availability of a product.

<NodeGraph />

<SchemaViewer
  file="schema.json"
  title="Schema"
  expand="true"
  maxHeight="500"
  search="true"
/>

### Query using CURL

```sh
curl -X GET "https://api.yourdomain.com/inventory/{productId}/status" \
  -H "Content-Type: application/json"
```
````

```

## Key Conventions

- Queries represent read-only requests (no side effects)
- Use `operation` field for REST-style queries to document the GET endpoint
- Include `<SchemaViewer />` for the response schema
- Include CURL or code examples showing how to call the query
```
