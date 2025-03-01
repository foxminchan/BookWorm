---
id: finance
name: Finance
version: 1.0.0
services:
  - id: Finance Service
    version: 1.0.0
badges:
  - content: Support Domain
    backgroundColor: purple
    textColor: purple
  - content: Event Sourcing
    backgroundColor: red
    textColor: red
owners:
  - nhanxnguyen
---

## Overview

The Finance domain serves as the backbone of BookWorm's monetary operations, handling all financial transactions across the platform. This support domain manages basket reservations, order processing workflows, payment processing, refund mechanisms, and financial reporting.

Key responsibilities include:

- Orchestrating the financial aspects of order fulfillment
- Managing payment gateway integrations
- Handling billing and invoicing processes
- Processing refunds and adjustments
- Providing financial reporting and analytics
- Ensuring compliance with financial regulations
- Managing discount and promotion calculations

The Finance domain communicates with multiple other domains through event-based messaging, particularly with Ordering and Basket services. It maintains its own ledger of transactions while providing a consistent financial view across the entire BookWorm platform.

<Tiles >
    <Tile icon="UserGroupIcon" href="/docs/users/nhanxnguyen" title="Contact the author" description="Any questions? Feel free to contact the owners" />
    <Tile icon="RectangleGroupIcon" href={`/visualiser/domains/${frontmatter.id}/${frontmatter.version}`} title={`${frontmatter.services.length} services are in this domain`} description="This service sends messages to downstream consumers" />
</Tiles>

## Bounded context

<NodeGraph />

<MessageTable format="all" limit={4} />
