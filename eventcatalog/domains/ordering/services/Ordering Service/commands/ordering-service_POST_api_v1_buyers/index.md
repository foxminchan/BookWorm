---
id: ordering-service_POST_api_v1_buyers
version: 1.0.0
name: Create Buyer
summary: Create buyer in the ordering domain
schemaPath: ""
badges:
  - content: POST
    textColor: green
    backgroundColor: green
  - content: "Buyer"
    textColor: yellow
    backgroundColor: yellow
owners:
  - nhanxnguyen
---

## Overview

Create a new buyer in the Ordering bounded context. The Buyer represents a crucial aggregate root in the Ordering domain model that encapsulates customer purchasing identity and payment methods.

## Domain Significance

In our domain model, a Buyer:

- Serves as an aggregate root with its own identity
- Maintains a collection of payment methods
- Associates with orders while preserving customer information
- Enforces invariants around required buyer information and valid payment methods

This endpoint allows clients to register a new Buyer entity in the system, which is a prerequisite for creating orders. The creation process validates that all required buyer attributes meet domain rules and generates a unique identifier for the new aggregate.

## Business Rules

- Each Buyer must have a unique identifier
- A Buyer must be associated with a valid user identity
- Payment methods added to a Buyer must contain valid payment information
- Buyer information is independently maintained from the customer profile in other bounded contexts

## Architecture

<NodeGraph />

## POST `(/api/v1/buyers)`

### Parameters

- **id** (path) (required): The order id

### Responses

#### <span className="text-green-500">200 OK</span>

<SchemaViewer file="response-200.json" maxHeight="500" id="response-200" />
