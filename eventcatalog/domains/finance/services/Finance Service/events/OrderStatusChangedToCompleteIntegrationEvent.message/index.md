---
id: orderstatuschangedtocompleteintegrationevent.message
version: 1.0.0
name: Changed Order Status To Complete
summary: Represents an integration event to notify the system about the status change of an order to `Complete`
badges: []
schemaPath: schema.json
owners:
	- nhanxnguyen
---

## Overview

This message represents an integration event to notify the system about the status change of an order to `Complete`. The `OrderStatusChangedToCompleteIntegrationEvent` is a domain event that captures the status change of an order in the Finance bounded context. It carries the necessary value objects including the order identity, the completion reason, and the total amount to notify the system about the status change operation. This message adheres to the ubiquitous language of our domain and serves as the contract between the Finance and external systems, facilitating the transition from an order to a completed one.

## Architecture

<NodeGraph />
