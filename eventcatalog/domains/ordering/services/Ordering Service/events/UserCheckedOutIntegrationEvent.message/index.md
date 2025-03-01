---
id: usercheckedoutintegrationevent.message
version: 1.0.0
name: User Checked Out
summary: Represents an integration event when a user has checked out
badges: []
schemaPath: schema.json
owners:
	- nhanxnguyen
---

## Overview

This event represents an integration event when a user has checked out. The `UserCheckedOutIntegrationEvent` is a domain event that captures the checkout process in the Ordering bounded context. It carries the necessary value objects including the order identity, the user identity, and the checkout date to notify the system about the checkout process. This event adheres to the ubiquitous language of our domain and serves as the contract between the Ordering and external systems, facilitating the transition from a pending order to a checked out one.

## Architecture

<NodeGraph />
