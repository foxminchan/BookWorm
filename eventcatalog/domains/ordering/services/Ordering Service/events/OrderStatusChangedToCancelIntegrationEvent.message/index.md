---
id: orderstatuschangedtocancelintegrationevent.message
version: 1.0.0
name: Changed Order Status To Cancel
summary: Represents an integration event when an order status is changed to cancel
badges: []
schemaPath: schema.json
owners:
	- nhanxnguyen
---

## Overview

This event represents an integration event when an order status is changed to cancel. The `OrderStatusChangedToCancelIntegrationEvent` is a domain event that captures the change of an order's status to cancel in the Ordering bounded context. It carries the necessary value objects including the order identity, the new status, and the cancellation reason to notify the system about the status change. This event adheres to the ubiquitous language of our domain and serves as the contract between the Ordering and external systems, facilitating the transition from a pending order to a cancelled one.

## Architecture

<NodeGraph />
