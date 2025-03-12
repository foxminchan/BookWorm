---
id: cancelordercommand.message
version: 1.0.0
name: Cancelled Order
summary: Represents a command to cancel an order in the system
badges:
  - content: Orchestrated
    textColor: orange
    backgroundColor: orange
schemaPath: schema.json
owners:
	- nhanxnguyen
---

## Overview

This message represents a command to cancel an order in the system. The `CancelOrderCommand` is a domain event that captures the request to cancel an order in the Finance bounded context. It carries the necessary value objects including the order identity, the cancellation reason, and the total amount to notify the system about the cancellation operation. This message adheres to the ubiquitous language of our domain and serves as the contract between the Finance and external systems, facilitating the transition from an order to a cancelled one.

## Architecture

<NodeGraph />
