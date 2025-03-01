---
id: basketdeletedcompleteintegrationevent.message
version: 1.0.0
name: Deleted Basket Complete
summary: Represents a domain event that is published when reverse basket is completed
badges: []
schemaPath: schema.json
owners:
  - nhanxnguyen
---

## Overview

This event represents a complete basket deletion event in the system. The `BasketDeletedCompleteIntegrationEvent` is a domain event that captures the successful deletion of a basket in the Finance bounded context. It carries the necessary value objects including the basket identity, the deleted items, and the total amount to notify the system about the successful operation. This event adheres to the ubiquitous language of our domain and serves as the contract between the Finance and external systems, facilitating the transition from a basket deletion to a successful one.

## Architecture

<NodeGraph />
