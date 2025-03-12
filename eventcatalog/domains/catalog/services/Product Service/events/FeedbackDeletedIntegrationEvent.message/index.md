---
id: feedbackdeletedintegrationevent.message
version: 1.0.0
name: Deleted Feedback
summary: Represents a successful integration event when deleting a feedback in the system
badges: []
schemaPath: schema.json
owners:
  - nhanxnguyen
---

## Overview

This event represents a successful integration event when deleting a feedback in the system. The `FeedbackDeletedIntegrationEvent` is a domain event that captures the deletion of a feedback in the Catalog bounded context. It carries the necessary value objects including the feedback identity to notify the system about the successful operation. This event adheres to the ubiquitous language of our domain and serves as the contract between the Catalog and external systems, facilitating the transition from a failed feedback deletion to a successful one.

## Architecture

<NodeGraph />
