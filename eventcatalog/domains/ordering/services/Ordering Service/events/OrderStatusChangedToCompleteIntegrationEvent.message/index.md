---
id: orderstatuschangedtocompleteintegrationevent.message
version: 1.0.0
name: Changed Order Status To Complete
summary: Represents an integration event when an order status is changed to complete
badges: []
schemaPath: schema.json
owners:
	- nhanxnguyen
---

## Overview

This event represents an integration event when an order status is changed to complete. The `OrderStatusChangedToCompleteIntegrationEvent` is a domain event that captures the change of an order's status to complete in the Ordering bounded context. It carries the necessary value objects including the order identity, the new status, and the completion reason to notify the system about the status change. This event adheres to the ubiquitous language of our domain and serves as the contract between the Ordering and external systems, facilitating the transition from a pending order to a completed one.

## Architecture

<NodeGraph />
