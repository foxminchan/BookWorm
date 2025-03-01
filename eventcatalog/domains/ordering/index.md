---
id: ordering
name: Ordering
version: 1.0.0
services:
  - id: Ordering Service
    version: 1.0.0
badges:
  - content: Core Domain
    backgroundColor: green
    textColor: green
  - content: Event Sourcing
    backgroundColor: red
    textColor: red
owners:
  - nhanxnguyen
---

## Overview

The Ordering domain represents a strategic core domain within the BookWorm system, responsible for managing the complete lifecycle of customer orders from creation through fulfillment. As a core domain, it embodies critical business capabilities that directly impact the organization's competitive advantage.

<Tiles >
    <Tile icon="UserGroupIcon" href="/docs/users/nhanxnguyen" title="Contact the author" description="Any questions? Feel free to contact the owners" />
    <Tile icon="RectangleGroupIcon" href={`/visualiser/domains/${frontmatter.id}/${frontmatter.version}`} title={`${frontmatter.services.length} services are in this domain`} description="This service sends messages to downstream consumers" />
</Tiles>

### Domain Characteristics

- **Bounded Context**: The Ordering domain has a well-defined boundary with explicit integration points with other domains like Basket, Catalog, and Notification through domain events.

- **Event Sourcing**: The domain implements event sourcing as its persistence mechanism, storing the complete history of order-related events rather than just the current state. This approach provides a comprehensive audit trail and enables temporal queries and state reconstruction at any point in time.

- **Aggregates and Entities**:
  - **Order** (aggregate root) - Controls the consistency boundaries for order operations
  - **OrderItem** (entity) - Books and quantities associated with an order
  - **OrderStatus** (value object) - Immutable representation of order state
  - **Address** (value object) - Immutable shipping details

### Domain Events

The domain produces and consumes various events including:

- `OrderCancelledEvent`
- `OrderCompletedEvent`
- `OrderPlacedEvent`

### Business Rules and Invariants

- Orders must contain at least one item to be submitted
- Order status transitions follow a predefined state machine (e.g., Placed -> Completed or Cancelled)
- Cancellation is only permitted before the "Shipped" status
- Price calculations and summaries are validated against business rules

The domain handles complex business scenarios like partial fulfillment, order modifications, and cancellations while maintaining consistency and enforcing business rules throughout the order lifecycle.

## Architecture diagram

<NodeGraph />

<MessageTable format="all" limit={4} />
