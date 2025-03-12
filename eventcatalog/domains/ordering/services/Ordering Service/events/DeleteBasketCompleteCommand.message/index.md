---
id: deletebasketcompletecommand.message
version: 1.0.0
name: Deleted Basket Complete
summary: Represents a domain event that is published when reverse basket is completed
badges: []
schemaPath: schema.json
owners:
	- nhanxnguyen
---

## Overview

This event represents a successful integration event when deleting a basket in the system. The `DeleteBasketCompleteCommand` is a domain event that captures the deletion of a basket in the Ordering bounded context. It carries the necessary value objects including the basket identity, the customer identity, and the basket items to notify the system about the successful operation. This event adheres to the ubiquitous language of our domain and serves as the contract between the Ordering and external systems, facilitating the transition from a failed basket deletion to a successful one.

## Architecture

<NodeGraph />
