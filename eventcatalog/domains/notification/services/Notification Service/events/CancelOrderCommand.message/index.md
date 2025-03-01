---
id: cancelordercommand.message
version: 1.0.0
name: Cancelled Order
summary: Receive a message when an order is canceled
badges: []
schemaPath: schema.json
owners:
	- nhanxnguyen
---

## Overview

This message represents a command to cancel an order. The `CancelOrderCommand` is a domain event that captures the cancellation operation in the Notification bounded context. It carries the necessary value objects including the order identity, the reason for cancellation, and the customer information to notify the system about the cancellation operation. This message adheres to the ubiquitous language of our domain and serves as the contract between the Notification and external systems, facilitating the transition from an order to a canceled one.

## Architecture

<NodeGraph />
