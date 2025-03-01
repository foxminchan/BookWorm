---
id: notification
name: Notification
version: 1.0.0
services:
  - id: Notification Service
    version: 1.0.0
badges:
  - content: Generic Domain
    backgroundColor: gray
    textColor: gray
  - content: Transaction Script
    backgroundColor: blue
    textColor: blue
owners:
  - nhanxnguyen
---

## Overview

The Notification domain is responsible for sending messages to external systems when an order is placed, completed, or canceled. It captures the necessary information about the order, including the order identity, the order date, and the customer information, to notify the system about the operation. This domain serves as the contract between the Notification and external systems, facilitating the transition from an order to a placed, completed, or canceled one.

<Tiles >
    <Tile icon="UserGroupIcon" href="/docs/users/nhanxnguyen" title="Contact the author" description="Any questions? Feel free to contact the owners" />
    <Tile icon="RectangleGroupIcon" href={`/visualiser/domains/${frontmatter.id}/${frontmatter.version}`} title={`${frontmatter.services.length} services are in this domain`} description="This service sends messages to downstream consumers" />
</Tiles>

## Responsibilities

- Send messages to external systems when an order is placed, completed, or canceled
- Capture the necessary information about the order, including the order identity, the order date, and the customer information
- Notify the system about the operation
- Serve as the contract between the Notification and external systems

## Bounded context

<NodeGraph />

<MessageTable format="all" limit={4} />
