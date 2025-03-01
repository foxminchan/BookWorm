---
id: deletebasketfailedcommand.message
version: 1.0.0
name: Deleted Basket Fail
summary: Represents a command to notify the system about the failure to delete a basket
badges:
  - content: Orchestrated
    textColor: orange
    backgroundColor: orange
schemaPath: schema.json
owners:
	- nhanxnguyen
---

## Overview

This message represents a command to notify the system about the failure to delete a basket. The `DeleteBasketFailedCommand` is a domain event that captures the request to fail the deletion of a basket in the Finance bounded context. It carries the necessary value objects including the basket identity, the failure reason, and the total amount to notify the system about the failure operation. This message adheres to the ubiquitous language of our domain and serves as the contract between the Finance and external systems, facilitating the transition from a basket deletion to a failed one.

## Architecture

<NodeGraph />
