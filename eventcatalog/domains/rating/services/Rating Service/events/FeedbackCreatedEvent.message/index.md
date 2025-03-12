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

The `FeedbackCreatedEvent` message is published by the Rating service to notify subscribers of a new feedback submission. This event is triggered when a user successfully submits a review or rating for a book within the BookWorm ecosystem. The event payload contains essential information about the feedback, including the user's identity, the book being reviewed, and the feedback content.

## Architecture

<NodeGraph />
