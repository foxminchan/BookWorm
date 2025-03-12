---
id: basket
name: Basket
version: 1.0.0
services:
  - id: Basket Service
    version: 1.0.0
badges:
  - content: Support Domain
    backgroundColor: purple
    textColor: purple
  - content: Active Record
    backgroundColor: green
    textColor: green
owners:
  - nhanxnguyen
---

## Overview

The Basket domain is responsible for managing the shopping basket of a customer throughout their shopping journey. It serves as a temporary storage for items that customers intend to purchase before proceeding to checkout and placing an order.

<Tiles >
    <Tile icon="UserGroupIcon" href="/docs/users/nhanxnguyen" title="Contact the author" description="Any questions? Feel free to contact the owners" />
    <Tile icon="RectangleGroupIcon" href={`/visualiser/domains/${frontmatter.id}/${frontmatter.version}`} title={`${frontmatter.services.length} services are in this domain`} description="This service sends messages to downstream consumers" />
</Tiles>

### Key Responsibilities

- Managing the addition, removal, and updating of items in a customer's basket
- Maintaining quantity information for each basket item
- Calculating pricing including subtotals, discounts, and estimated taxes
- Supporting the transition from anonymous to authenticated baskets when users log in
- Providing persistence for basket state between sessions
- Implementing basket expiration and cleanup processes
- Validating product availability and price consistency with the Catalog service
- Facilitating the checkout process by transferring basket data to the Ordering service

The domain uses an Active Record pattern for simplicity and performance, as basket operations need to be fast and highly available. The basket data is temporarily stored and doesn't require complex domain modeling as found in other services.

## Bounded context

<NodeGraph />

<MessageTable format="all" limit={4} />
