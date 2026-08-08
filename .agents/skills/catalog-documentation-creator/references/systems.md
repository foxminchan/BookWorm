# Systems

## Format

**File:** `index.mdx` inside a system folder
**Location:** `systems/{SystemName}/index.mdx`, `domains/{Domain}/systems/{SystemName}/index.mdx`, or `domains/{Domain}/subdomains/{Subdomain}/systems/{SystemName}/index.mdx`

Systems document software capabilities or external systems. A system is a collection of resources that work together to perform a function. Systems often sit inside domains, but shared or third-party systems can live at the catalog root.

## Frontmatter Fields

| Field           | Required | Description                                                                                   |
| --------------- | -------- | --------------------------------------------------------------------------------------------- |
| `id`            | Yes      | Unique system identifier, often kebab-case (e.g., `cart-system`, `payment-processing-system`) |
| `name`          | Yes      | Human-readable name                                                                           |
| `version`       | Yes      | Semver string (e.g., `1.0.0`)                                                                 |
| `summary`       | No       | 1-2 sentence description of the system                                                        |
| `owners`        | No       | Array of team or user IDs                                                                     |
| `scope`         | No       | `internal` or `external`. Defaults to `internal`                                              |
| `services`      | No       | Array of services that belong to the system                                                   |
| `containers`    | No       | Array of data stores/containers that belong to the system                                     |
| `flows`         | No       | Array of flows that belong to the system                                                      |
| `entities`      | No       | Array of entities that belong to the system                                                   |
| `relationships` | No       | One-directional relationships from this system to other systems                               |
| `actors`        | No       | People, roles, or external participants that interact with the system                         |
| `diagrams`      | No       | Diagrams associated with the system                                                           |
| `attachments`   | No       | External links or supporting documents                                                        |
| `repository`    | No       | Object with `language` and `url`                                                              |
| `badges`        | No       | Array of badge objects                                                                        |
| `deprecated`    | No       | Object with `date` and `message` for deprecated systems                                       |

## Folder Structure

Systems can live at the root:

```
systems/stripe/index.mdx
systems/checkout-system/services/CheckoutAPI/index.mdx
systems/checkout-system/containers/checkout-database/index.mdx
```

Systems can also live inside domains:

```
domains/Shopping/systems/cart-system/index.mdx
domains/Shopping/systems/cart-system/services/CartAPI/index.mdx
domains/Shopping/systems/cart-system/containers/cart-database/index.mdx
domains/Shopping/systems/cart-system/flows/CheckoutFlow/index.mdx
```

Versioned systems use:

```
systems/{SystemName}/versioned/{version}/index.mdx
domains/{Domain}/systems/{SystemName}/versioned/{version}/index.mdx
```

## Resource Pointers

Systems reference resources using `id` and optional `version`. If `version` is omitted, EventCatalog uses the latest version.

```yaml
services:
  - id: cart-api
    version: 1.0.0
containers:
  - id: cart-database
flows:
  - id: checkout-flow
entities:
  - id: cart
diagrams:
  - id: cart-deployment
```

Messages, APIs, schemas, and channels are usually documented through the services and messages inside the system rather than directly on the system.

## Relationships

Use `relationships` to connect this system to other systems. Relationships are one-directional: define the relationship on the source system.

| Field     | Required | Description                               |
| --------- | -------- | ----------------------------------------- |
| `id`      | Yes      | Target system ID                          |
| `version` | No       | Target system version. Defaults to latest |
| `label`   | No       | Text shown on the relationship edge       |

```yaml
relationships:
  - id: promotion-system
    version: 1.0.0
    label: calculates discounts via
```

## Actors

Use `actors` for people, roles, or external participants that interact with a system. Actors are inline on systems, not separate collection files.

| Field       | Required | Description                                                                              |
| ----------- | -------- | ---------------------------------------------------------------------------------------- |
| `id`        | Yes      | Unique actor ID, shared across the catalog for deduplication                             |
| `name`      | No       | Display name shown in diagrams                                                           |
| `label`     | No       | Text shown on the actor relationship edge                                                |
| `direction` | No       | `inbound` means actor to system. `outbound` means system to actor. Defaults to `inbound` |

```yaml
actors:
  - id: shopper
    name: Shopper
    label: adds items and checks out
    direction: inbound
  - id: customer
    name: Customer
    label: receives checkout confirmation
    direction: outbound
```

## Example: Internal System Inside a Domain

```mdx
---
id: cart-system
name: Cart System
version: 1.0.0
summary: |
  Internal system that owns the customer's shopping cart and checkout flow.
scope: internal
owners:
  - shopping-platform
services:
  - id: cart-api
  - id: cart-worker
containers:
  - id: cart-database
flows:
  - id: checkout-flow
entities:
  - id: cart
relationships:
  - id: promotion-system
    label: calculates discounts via
actors:
  - id: shopper
    name: Shopper
    label: adds items and checks out
    direction: inbound
badges:
  - content: Internal
    backgroundColor: gray
    textColor: gray
---

## Overview

The Cart System owns active shopping carts. It accepts commands to add and remove items, persists cart state in [[container|cart-database]], asks [[system|promotion-system]] to calculate discounts, and publishes checkout events through [[service|cart-api]].

## Context Diagram

<ContextDiagram />

## Resource Diagram

<NodeGraph />

## What's inside

| Component   | Type            | Responsibility |
| ----------- | --------------- | -------------- | --------------------------------------------- |
| [[service   | cart-api]]      | Service        | Public API for cart reads and writes.         |
| [[service   | cart-worker]]   | Service        | Handles asynchronous cart expiry and cleanup. |
| [[container | cart-database]] | Data store     | Stores active carts and checkout state.       |
```

## Example: External System

```mdx
---
id: stripe
name: Stripe
version: 1.0.0
summary: External payment provider used for card authorization, capture, and refunds.
scope: external
owners:
  - payments-platform
relationships:
  - id: payment-processing-system
    label: receives payment requests from
attachments:
  - title: Stripe dashboard
    url: https://dashboard.stripe.com
    type: dashboard
styles:
  icon: https://cdn.simpleicons.org/stripe
---

## Overview

Stripe is the external payment system used by [[system|payment-processing-system]] for card payment authorization, capture, and refund workflows.

<ContextDiagram />
```

## Domain Membership

When a system belongs to a domain, add it to the domain's `systems` frontmatter:

```yaml
systems:
  - id: cart-system
    version: 1.0.0
```

If a system is created under `domains/{Domain}/systems/{System}/` but is not listed in the domain frontmatter, it will not appear as part of the domain. Always cross-check after generating files.

## Key Conventions

- Use systems for product capabilities, software boundaries, or external systems, not for a single message or schema.
- Prefer domain-nested systems when the system clearly belongs to one domain.
- Prefer root-level systems when the system is shared, external, or can be attached to domains later.
- Use `scope: external` for third-party/SaaS systems; omit `scope` or use `scope: internal` for systems owned by the organization.
- Use `[[system|SystemName]]` syntax to link to other systems in the body.
- Include `<ContextDiagram />` when `relationships` or `actors` are defined.
- Include `<NodeGraph />` to show the services, containers, flows, and entities inside the system.
- Generate contained resources inside the system folder when the system owns them.
- Do not put messages directly in system frontmatter; model messages through the services inside the system.
