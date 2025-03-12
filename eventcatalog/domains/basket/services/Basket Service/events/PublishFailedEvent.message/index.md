---
id: basketdeletedfailedintegrationevent.message
version: 1.0.0
name: Deleted Basket Fail
summary: Represents a failed basket deletion event in the system
badges: []
schemaPath: schema.json
owners:
  - nhanxnguyen
---

## Overview

This message represents a domain event that is published when a reverse basket is failed. The `PublishFailedEvent` is a domain event that signals the failure of a reverse basket in the system. It carries the necessary value objects including the reverse basket identity, customer identity, and basket item collection to create a valid reverse basket aggregate. This event adheres to the ubiquitous language of our domain and serves as the contract between the Basket and Ordering contexts, facilitating the transition from a transient basket to a persistent order.

## Architecture

<NodeGraph />
