---
id: placeordercommand.message
version: 1.0.0
name: Placed Order
summary: Represents a command to place an order
badges: []
schemaPath: schema.json
owners:
  - nhanxnguyen
---

## Overview

This message represents a command to place an order in the system. The `PlaceOrderCommand` is a domain command that initiates the order aggregate creation process within the Ordering bounded context. It carries the necessary value objects including customer identity, shipping address value object, and basket item collection to create a valid order aggregate. This command adheres to the ubiquitous language of our domain and serves as the contract between the Basket and Ordering contexts, facilitating the transition from a transient basket to a persistent order.

## Architecture

<NodeGraph />
