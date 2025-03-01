---
id: bookupdatedratingfailedintegrationevent.message
version: 1.0.0
name: Updated Book Rating Fail
summary: Represents a failed integration event when updating a book's rating in the system
badges: []
schemaPath: schema.json
owners:
  - nhanxnguyen
---

## Overview

This event represents a failed integration event when updating a book's rating in the system. The `BookUpdatedRatingFailedIntegrationEvent` is a domain event that captures the failure to update a book's rating in the Catalog bounded context. It carries the necessary value objects including the book identity, the failed rating value, and the error message to notify the system about the failed operation. This event adheres to the ubiquitous language of our domain and serves as the contract between the Catalog and external systems, facilitating the transition from a successful rating update to a failed one.

## Architecture

<NodeGraph />
