---
id: "rating-service_DELETE_api_v1_feedbacks_{id}"
version: 1.0.0
name: Delete Feedback
summary: Delete feedback for a book in the Rating Service
schemaPath: ""
badges:
  - content: DELETE
    textColor: red
    backgroundColor: red
  - content: "Feedback"
    textColor: yellow
    backgroundColor: yellow
owners:
	- nhanxnguyen
---

## Overview

The Delete Feedback operation allows removing customer feedback from the Rating domain. This endpoint is part of the Rating bounded context and operates on the Feedback aggregate.

### Domain Context

Within our domain model, Feedback represents a customer's opinion about a book, which is an important entity in our system. Deleting feedback is a domain operation that:

1. Validates the feedback exists and belongs to the authenticated user
2. Removes the feedback entity from the repository
3. Raises a `FeedbackDeletedDomainEvent` for other bounded contexts to react to

### Command Flow

When a DELETE request is received:

1. The `DeleteFeedbackCommand` is dispatched through the mediator
2. The command handler retrieves the feedback entity from the repository
3. Authorization checks ensure the requesting user has permission to delete
4. The feedback is removed from the repository
5. A domain event is published to notify other services

### Business Rules

- Only the feedback author or administrators can delete feedback
- Deleted feedback cannot be recovered
- Deleting feedback recalculates the aggregate rating for the associated book

This operation maintains the integrity of our Rating domain while allowing users to manage their own content within the system.

## Architecture

<NodeGraph />

## DELETE `(/api/v1/feedbacks/{id})`

### Responses

#### <span className="text-green-500">200 OK</span>

<SchemaViewer file="response-200.json" maxHeight="500" id="response-200" />

#### <span className="text-orange-500">400 Bad Request</span>

<SchemaViewer file="response-400.json" maxHeight="500" id="response-400" />
