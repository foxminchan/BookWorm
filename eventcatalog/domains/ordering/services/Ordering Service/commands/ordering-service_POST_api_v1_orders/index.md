---
id: ordering-service_POST_api_v1_orders
version: 1.0.0
name: Create Order
summary: Create order for a customer
schemaPath: ""
badges:
  - content: POST
    textColor: green
    backgroundColor: green
  - content: "Order"
    textColor: yellow
    backgroundColor: yellow
owners:
  - nhanxnguyen
---

## Overview

The Create Order endpoint represents a key aggregate root operation within the Ordering bounded context. This command follows the CQRS pattern and is responsible for initiating a new order in the system.

### Domain Concepts

- **Order Aggregate**: Central entity containing order items, delivery information, and payment details
- **Order Items**: Value objects representing products with quantity and price information
- **Customer Reference**: External identifier linking to the Customer bounded context
- **Order Status**: State machine representing the lifecycle of an order (Created → Processing → Shipped → Delivered)

### Business Rules

- Orders must contain at least one valid order item
- Orders must reference an existing customer
- Total order value is calculated as the sum of all item prices multiplied by quantities
- Initial order status is set to "Created" upon successful submission
- Inventory verification happens asynchronously via domain events

### Domain Events

Upon successful order creation, the following domain events are published:

- `OrderCreatedEvent`: Triggers inventory checks and payment processing

### Implementation Notes

The order creation process follows a two-phase commit pattern to ensure consistency across bounded contexts. The command handler validates input, creates the Order aggregate, and persists it to the database before publishing domain events.

## Architecture

<NodeGraph />

## POST `(/api/v1/orders)`

### Parameters

- **id** (path) (required): The order id

### Responses

#### <span className="text-green-500">200 OK</span>

<SchemaViewer file="response-200.json" maxHeight="500" id="response-200" />
