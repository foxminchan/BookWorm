---
id: deletebasketfailedcommand.message
version: 1.0.0
name: Deleted Basket Fail
summary: Represents a failed integration event when deleting a basket in the system
badges: []
schemaPath: schema.json
owners:
	- nhanxnguyen
---

## Overview

This event represents a failed integration event when deleting a basket in the system. The `DeleteBasketFailedCommand` is a domain event that captures the failure of a basket deletion in the Ordering bounded context. It carries the necessary value objects including the basket identity, the customer identity, and the basket items to notify the system about the failed operation. This event adheres to the ubiquitous language of our domain and serves as the contract between the Ordering and external systems, facilitating the transition from a successful basket deletion to a failed one.

## Architecture

<NodeGraph />
