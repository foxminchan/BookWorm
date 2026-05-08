# Domains

## Format

**File:** `index.mdx` inside a domain folder
**Location:** `domains/{DomainName}/index.mdx`

Domains can contain subdomains, services, flows, and other resources nested inside them.

## Frontmatter Fields

| Field           | Required | Description                                                                                    |
| --------------- | -------- | ---------------------------------------------------------------------------------------------- |
| `id`            | Yes      | Unique identifier (e.g., `E-Commerce`, `Payment`)                                              |
| `name`          | Yes      | Human-readable name                                                                            |
| `version`       | Yes      | Semver string (e.g., `0.0.1`)                                                                  |
| `summary`       | Yes      | Description of the domain's purpose and scope                                                  |
| `owners`        | Yes      | Array of team or user IDs                                                                      |
| `domains`       | No       | Array of subdomain references (`id`)                                                           |
| `services`      | No       | Array of service references (only needed for flat structure, nested folders are auto-detected) |
| `data-products` | No       | Array of data product references                                                               |
| `badges`        | No       | Array of badge objects                                                                         |
| `repository`    | No       | Object with `language` and `url`                                                               |
| `sends`         | No       | Messages sent at the domain level                                                              |
| `receives`      | No       | Messages received at the domain level                                                          |

## Nested Folder Structure

Domains support deep nesting:

```
domains/E-Commerce/
  index.mdx                          # Domain definition
  subdomains/
    Orders/
      index.mdx                      # Subdomain definition
      services/
        OrdersService/
          index.mdx
          events/
            OrderCreated/index.mdx
          commands/
            PlaceOrder/index.mdx
    Payment/
      index.mdx
      services/
        PaymentService/index.mdx
```

## Example: Domain with Subdomains and Rich Content

```mdx
---
id: E-Commerce
name: E-Commerce
version: 1.0.0
summary: The E-Commerce domain is the core business domain of FlowMart, our modern digital marketplace. This domain orchestrates all critical business operations from product discovery to order fulfillment, handling millions of transactions monthly across our global customer base.
owners:
  - dboyne
  - full-stack
domains:
  - id: Orders
  - id: Payment
  - id: Subscriptions
data-products:
  - id: order-analytics
badges:
  - content: Core domain
    backgroundColor: blue
    textColor: blue
    icon: RectangleGroupIcon
  - content: Business Critical
    backgroundColor: yellow
    textColor: yellow
    icon: ShieldCheckIcon
repository:
  language: TypeScript
  url: "https://github.com/event-catalog/pretend-e-commerce-domain"
sends:
  - id: PaymentComplete
  - id: UserSubscriptionStarted
receives:
  - id: PaymentInitiated
  - id: FraudDetected
---

## Domain Overview

The E-Commerce domain encapsulates all the core business logic for the FlowMart e-commerce platform. It is built on event-driven microservices architecture with key services like [[service|OrdersService]], [[service|InventoryService]], and [[service|PaymentService]].

<NodeGraph mode="full" search="false" legend="false" />

FlowMart's E-Commerce domain enables:

- Real-time inventory management via [[service|InventoryService]] across multiple warehouses
- Seamless payment processing with [[service|PaymentService]] and [[service|PaymentGatewayService]]
- Smart order routing and fulfillment through [[service|OrdersService]]

## Core Subdomains

- [[domain|Orders]] - Core domain for order management
- [[domain|Payment]] - Payment processing using Stripe
- [[domain|Subscriptions]] - Subscription handling

## Key Business Flows

| Flow              | Description                                   |
| ----------------- | --------------------------------------------- |
| Order Processing  | Customer places order through fulfillment     |
| Returns & Refunds | Customer initiates return and receives refund |

## Performance SLAs

- Order Processing: < 2 seconds
- Payment Processing: < 3 seconds
- Inventory Updates: Real-time
```

## Key Conventions

- Use `domains` field (not `services`) when a domain has subdomains
- Use `[[service|ServiceName]]` and `[[domain|DomainName]]` syntax to create links
- Use `<NodeGraph mode="full" />` for a comprehensive domain visualization
- Include sequence diagrams (`mermaid`) for complex flows
- Domains can have their own `sends`/`receives` to show domain-level message flow
