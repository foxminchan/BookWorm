---
id: rating-service_GET_api_v1_feedbacks
version: 1.0.0
name: List Feedbacks
summary: Get feedbacks for a book
schemaPath: ""
badges:
  - content: GET
    textColor: blue
    backgroundColor: blue
  - content: "Feedback"
    textColor: yellow
    backgroundColor: yellow
owners:
	- nhanxnguyen
---

## Overview

The Feedback retrieval endpoint is a core read operation within the Rating bounded context, responsible for fetching book feedback aggregates. This endpoint implements a query-side operation following CQRS principles, allowing clients to access feedback data without impacting the command-side of the Rating domain.

### Domain Context

In our domain model, `Feedback` represents a valuable domain concept that captures reader opinions and ratings for books in the catalog. Each feedback belongs to a specific book and contains:

- Rating score (numerical evaluation)
- Review content (textual feedback)
- Metadata (submission date, reviewer information)

### Implementation Details

The endpoint leverages a repository pattern to fetch feedback entities and projects them into DTOs before returning to the client. The pagination functionality optimizes resource utilization and response time by limiting result sets to manageable chunks. The ordering capabilities allow clients to sort feedback based on domain-relevant properties (e.g., submission date, rating score).

### Integration Points

This endpoint serves as an integration point for other bounded contexts within the BookWorm ecosystem:

- The Catalog service may consume this data to display aggregated ratings
- The Recommendation engine might analyze feedback patterns to suggest books

### Authorization

Access to feedback data follows our domain authorization rules, ensuring that published feedback is available while respecting privacy constraints defined in our domain policies.

## Architecture

<NodeGraph />

## GET `(/api/v1/feedbacks)`

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
