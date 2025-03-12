---
id: feedbackcreatedintegrationevent.message
version: 1.0.0
name: Created Feedback
summary: Represents a successful integration event when creating a feedback in the system
badges: []
schemaPath: schema.json
owners:
  - nhanxnguyen
---

## Overview

This event represents a successful integration event when creating a feedback in the system. The `FeedbackCreatedIntegrationEvent` is a domain event that captures the creation of a feedback in the Catalog bounded context. It carries the necessary value objects including the feedback identity, the feedback content, and the feedback rating to notify the system about the successful operation. This event adheres to the ubiquitous language of our domain and serves as the contract between the Catalog and external systems, facilitating the transition from a failed feedback creation to a successful one.

## Architecture

<NodeGraph />
