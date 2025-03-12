---
id: usercheckedoutintegrationevent.message
version: 1.0.0
name: User Checked Out
summary: Represents an integration event to notify the system about the user checkout operation
badges: []
schemaPath: schema.json
owners:
	- nhanxnguyen
---

## Overview

This message represents an integration event to notify the system about the user checkout operation. The `UserCheckedOutIntegrationEvent` is a domain event that captures the checkout operation in the Finance bounded context. It carries the necessary value objects including the order identity, the total amount, and the customer information to notify the system about the checkout operation. This message adheres to the ubiquitous language of our domain and serves as the contract between the Finance and external systems, facilitating the transition from an order to a completed one.

## Architecture

<NodeGraph />
