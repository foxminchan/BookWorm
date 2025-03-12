---
id: placeordercommand.message
version: 1.0.0
name: Placed Order
summary: Represents a command to notify the system about the placement of an order
badges:
  - content: Orchestrated
    textColor: orange
    backgroundColor: orange
schemaPath: schema.json
owners:
	- nhanxnguyen
---

## Overview

This message represents a command to notify the system about the placement of an order. The `PlaceOrderCommand` is a domain event that captures the request to place an order in the Finance bounded context. It carries the necessary value objects including the order identity, the order items, the total amount, and the customer information to notify the system about the placement operation. This message adheres to the ubiquitous language of our domain and serves as the contract between the Finance and external systems, facilitating the transition from a basket to an order.

## Architecture

<NodeGraph />
