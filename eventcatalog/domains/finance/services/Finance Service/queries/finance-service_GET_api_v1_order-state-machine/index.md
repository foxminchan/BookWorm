---
id: finance-service_GET_api_v1_order-state-machine
version: 1.0.0
name: Get Order State Machine
summary: Get order state machine
schemaPath: ""
badges:
  - content: GET
    textColor: blue
    backgroundColor: blue
  - content: "OrderState"
    textColor: yellow
    backgroundColor: yellow
owners:
  - nhanxnguyen
---

## Overview

This API endpoint provides access to the Order State Machine, which is a core component of our domain model following Domain-Driven Design principles. The Order State Machine represents the lifecycle of an order as it moves through various states in our bounded context.

The state machine implements a formal workflow pattern that enforces business rules and transitional logic between order states. Each state transition is triggered by specific domain events and may fire subsequent events when completed successfully.

This endpoint allows clients to retrieve the current configuration of the state machine, including:

- Valid states within the Order aggregate
- Permitted transitions between states
- Guards and conditions that must be satisfied for transitions
- Side effects that occur during transitions

This resource is particularly useful for UI components that need to display available actions based on the current state of an order.

## Architecture

<NodeGraph />

## GET `(/api/v1/order-state-machine)`

### Responses

#### <span className="text-green-500">200 OK</span>

<SchemaViewer file="response-200.json" maxHeight="500" id="response-200" />
