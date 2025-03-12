---
id: deletebasketcompletecommand.message
version: 1.0.0
name: Deleted Basket Complete
summary: Represents a command to complete the deletion of a basket in the system
badges:
  - content: Orchestrated
    textColor: orange
    backgroundColor: orange
schemaPath: schema.json
owners:
	- nhanxnguyen
---

## Overview

This message represents a command to complete the deletion of a basket in the system. The `DeleteBasketCompleteCommand` is a domain event that captures the request to complete the deletion of a basket in the Finance bounded context. It carries the necessary value objects including the basket identity, the deleted items, and the total amount to notify the system about the completion operation. This message adheres to the ubiquitous language of our domain and serves as the contract between the Finance and external systems, facilitating the transition from a basket deletion to a completed one.

## Architecture

<NodeGraph />
