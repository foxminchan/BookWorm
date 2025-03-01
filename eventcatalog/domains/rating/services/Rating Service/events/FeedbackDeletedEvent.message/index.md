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

The `FeedbackDeletedEvent` message is published by the Rating service to notify subscribers of a deleted feedback submission. This event is triggered when a user successfully deletes a review or rating for a book within the BookWorm ecosystem. The event payload contains essential information about the feedback, including the user's identity, the book being reviewed, and the feedback content.

## Architecture

<NodeGraph />
