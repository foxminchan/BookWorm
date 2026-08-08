# Entities

## Format

**File:** `index.mdx` inside an entity folder
**Location:** `entities/{EntityName}/index.mdx`, `domains/{Domain}/entities/{EntityName}/index.mdx`, `services/{Service}/entities/{EntityName}/index.mdx`, or nested under a system (`systems/{System}/entities/{EntityName}/index.mdx`, `domains/{Domain}/systems/{System}/entities/{EntityName}/index.mdx`)

Entities document domain concepts, aggregates, business objects, and persisted models. They are useful for DDD catalogs, data model documentation, and service-owned concepts.

## Frontmatter Fields

| Field           | Required | Description                                                 |
| --------------- | -------- | ----------------------------------------------------------- |
| `id`            | Yes      | Unique identifier, typically PascalCase (e.g., `Order`)     |
| `name`          | Yes      | Human-readable name                                         |
| `version`       | Yes      | Semver string (e.g., `1.0.0`)                               |
| `summary`       | Yes      | 1-2 sentence description                                    |
| `owners`        | No       | Array of team or user IDs                                   |
| `identifier`    | No       | Primary identifier property name (e.g., `orderId`)          |
| `aggregateRoot` | No       | Boolean indicating whether this entity is an aggregate root |
| `properties`    | No       | Array of property definitions                               |
| `badges`        | No       | Array of badge objects                                      |
| `repository`    | No       | Object with `language` and `url`                            |

## Property Fields

| Field                  | Required | Description                                                    |
| ---------------------- | -------- | -------------------------------------------------------------- |
| `name`                 | Yes      | Property name                                                  |
| `type`                 | Yes      | Property type                                                  |
| `required`             | No       | Boolean                                                        |
| `description`          | No       | Human-readable explanation                                     |
| `references`           | No       | Referenced entity ID                                           |
| `referencesIdentifier` | No       | Referenced entity identifier field                             |
| `relationType`         | No       | Relationship label such as `hasOne`, `hasMany`, or `belongsTo` |
| `enum`                 | No       | Allowed values                                                 |
| `items`                | No       | Object describing array item type                              |

## Example

```mdx
---
id: Order
name: Order
version: 1.0.0
identifier: orderId
aggregateRoot: true
summary: Represents a customer's request to purchase products or services.
properties:
  - name: orderId
    type: UUID
    required: true
    description: Unique identifier for the order
  - name: customerId
    type: UUID
    required: true
    description: Identifier for the customer placing the order
    references: Customer
    referencesIdentifier: customerId
    relationType: hasOne
  - name: status
    type: string
    required: true
    description: Current status of the order
    enum: ["Pending", "Processing", "Shipped", "Delivered", "Cancelled"]
  - name: orderItems
    type: array
    items:
      type: OrderItem
    required: true
    references: OrderItem
    referencesIdentifier: orderItemId
    relationType: hasMany
    description: List of items included in the order
---

## Overview

The Order entity captures all details related to a customer's purchase request. It serves as the central aggregate root within the Orders domain.

## Entity Properties

<EntityPropertiesTable />

## Relationships

- `Customer`: each order belongs to one customer.
- `OrderItem`: an order contains one or more order items.
```

## Key Conventions

- Use `aggregateRoot: true` only for aggregate roots.
- Use `identifier` for the primary identifier property.
- Use `references`, `referencesIdentifier`, and `relationType` to model relationships.
- Include `<EntityPropertiesTable />` when `properties` are documented.
- Link entities from domain, service, or system frontmatter:
  ```yaml
  entities:
    - id: Order
  ```
- Use `[[entity|Order]]` or `[[Order]]` in prose to link to entities.
