---
id: rating-service_POST_api_v1_feedbacks
version: 1.0.0
name: Create Feedback
summary: Create feedback for a book
schemaPath: ""
badges:
  - content: POST
    textColor: green
    backgroundColor: green
  - content: "Feedback"
    textColor: yellow
    backgroundColor: yellow
owners:
	- nhanxnguyen
---

## Overview

The `Create Feedback` endpoint allows users to submit reviews and ratings for books within the BookWorm ecosystem. This operation is part of the Rating domain's bounded context, responsible for managing user feedback and book evaluations.

### Domain Significance

In our domain model, `Feedback` represents a core aggregate that encapsulates:

- User evaluations (ratings on a defined scale)
- Textual reviews with content moderation rules
- Metadata about the feedback creator and timestamp
- Relationships to the Catalog domain via BookId

### Business Rules

- Users can only provide feedback for published books
- Feedback must include either a rating, a review, or both
- A user can submit only one feedback per book (subsequent submissions update existing feedback)
- Ratings must fall within the designated scale (1-5 stars)

### Integration Points

This endpoint interacts with the Catalog service to validate book existence and status before accepting feedback. The Rating service maintains its own projection of book data to minimize cross-service dependencies.

## Architecture

<NodeGraph />

## POST `(/api/v1/feedbacks)`

### Parameters

- **BookId** (query) (required): The ID of the book to get feedback for
- **PageIndex** (query): Number of items to return in a single page of results
- **PageSize** (query): Number of items to return in a single page of results
- **OrderBy** (query): Property to order results by
- **IsDescending** (query): Whether to order results in descending order

### Responses

#### <span className="text-green-500">200 OK</span>

<SchemaViewer file="response-200.json" maxHeight="500" id="response-200" />

#### <span className="text-orange-500">400 Bad Request</span>

<SchemaViewer file="response-400.json" maxHeight="500" id="response-400" />
