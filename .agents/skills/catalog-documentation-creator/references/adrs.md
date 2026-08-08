# Architecture Decision Records

## Format

**File:** `index.mdx` inside an ADR folder
**Location:** `adrs/{adr-id}/index.mdx`, or nested near the resource it applies to, e.g. `domains/{Domain}/systems/{System}/adrs/{adr-id}/index.mdx`

ADRs are first-class, versioned EventCatalog resources for documenting significant architecture decisions and linking them to the resources they affect.

## Frontmatter Fields

| Field            | Required | Description                                                                                            |
| ---------------- | -------- | ------------------------------------------------------------------------------------------------------ |
| `id`             | Yes      | Stable ADR identifier, typically kebab-case with a number (e.g., `adr-001-choose-event-driven-orders`) |
| `name`           | Yes      | Human-readable title, often prefixed with the ADR number                                               |
| `version`        | Yes      | Semver string (e.g., `1.0.0`)                                                                          |
| `summary`        | Yes      | 1-2 sentence decision summary                                                                          |
| `status`         | Yes      | One of `proposed`, `accepted`, `rejected`, `deprecated`, `superseded`                                  |
| `date`           | Yes      | Decision date in `YYYY-MM-DD` format                                                                   |
| `owners`         | No       | Team or user IDs responsible for maintaining the ADR                                                   |
| `decisionMakers` | No       | Team or user IDs who made or approved the decision                                                     |
| `appliesTo`      | No       | Resources impacted by the decision                                                                     |
| `supersedes`     | No       | ADRs this ADR replaces                                                                                 |
| `supersededBy`   | No       | ADRs that replace this ADR                                                                             |
| `amends`         | No       | ADRs this ADR partially changes                                                                        |
| `amendedBy`      | No       | ADRs that partially change this ADR                                                                    |
| `related`        | No       | Related ADRs                                                                                           |
| `badges`         | No       | Array of badge objects                                                                                 |

## Applies To Types

The `appliesTo` field supports these resource types:

- `agent`
- `service`
- `event`
- `command`
- `query`
- `flow`
- `channel`
- `domain`
- `system`
- `user`
- `team`
- `container`
- `entity`
- `diagram`
- `data-product`

Each item requires `type` and `id`; `version` is optional and defaults to `latest`.

## Example

```mdx
---
id: adr-001-choose-event-driven-orders
name: "ADR-001: Choose event-driven order processing"
version: 1.0.0
summary: Orders are processed through domain events so payment, inventory, and shipping can evolve independently.
status: accepted
date: 2026-05-26
owners:
  - order-management
decisionMakers:
  - order-management
appliesTo:
  - type: domain
    id: Orders
  - type: system
    id: checkout-system
  - type: service
    id: OrdersService
  - type: event
    id: OrderConfirmed
badges:
  - content: Messaging
    backgroundColor: blue
    textColor: blue
---

## Context

Order processing coordinates payment, inventory, fulfillment, and customer notifications. A synchronous workflow would couple these capabilities to the order service and make downstream changes harder to release independently.

## Decision

The Orders domain will publish domain events for important order lifecycle changes. Downstream services consume those events and own their local processing, retries, and failure handling.

## Consequences

Teams can add or change downstream processing without changing the order service contract for every workflow change. Consumers must handle eventual consistency and duplicate event delivery.
```

## Example: Superseded ADR

```mdx
---
id: adr-004-reserve-inventory-before-payment
name: "ADR-004: Authorize payment before reserving inventory"
version: 1.0.0
summary: The first checkout design authorized payment before inventory reservation.
status: superseded
date: 2023-11-17
owners:
  - order-management
decisionMakers:
  - order-management
appliesTo:
  - type: flow
    id: PlaceOrder
  - type: service
    id: PaymentService
  - type: service
    id: InventoryService
supersededBy:
  - id: adr-004-reserve-inventory-before-payment
    version: 2.0.0
---

## Context

Early checkout work optimized for fast payment confirmation and deferred stock allocation to the fulfillment workflow.

## Decision

Payment authorization happened before inventory reservation.

## Consequences

The approach produced poor customer outcomes when stock was exhausted after payment authorization. It has been superseded by reserving inventory before capture.
```

## Key Conventions

- Use `accepted` only when the decision is active and agreed.
- Use `proposed` for decisions still under review.
- Use `superseded` when a newer ADR replaces the decision, and include `supersededBy`.
- Use `deprecated` when a decision is no longer recommended but not directly replaced.
- Prefer `Context`, `Decision`, and `Consequences` headings in the body.
- Link affected resources with `appliesTo` so ADRs appear on related resource pages.
- ADRs that mainly describe a system boundary, persistence model, integration pattern, or ownership model can live under that system's `adrs/` folder and should include `type: system` in `appliesTo`.
- Keep ADR IDs stable across versions.
