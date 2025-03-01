---
id: basketdeletedfailedintegrationevent.message
version: 1.0.0
name: Deleted Basket Fail
summary: 	Represents a failed basket deletion event in the system
badges: []
schemaPath: schema.json
owners:
	- nhanxnguyen
---

## Overview

This event represents a failed basket deletion event in the system. The `BasketDeletedFailedIntegrationEvent` is a domain event that captures the failed deletion of a basket in the Finance bounded context. It carries the necessary value objects including the basket identity, the failed reason, and the total amount to notify the system about the failed operation. This event adheres to the ubiquitous language of our domain and serves as the contract between the Finance and external systems, facilitating the transition from a basket deletion to a failed one.

## Architecture

<NodeGraph />
