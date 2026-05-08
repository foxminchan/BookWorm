# Events

## Format

**File:** `index.mdx` inside an event folder
**Location:** `events/{EventName}/index.mdx` or nested under a service `domains/{Domain}/services/{Service}/events/{EventName}/index.mdx`

## Frontmatter Fields

| Field        | Required | Description                                                    |
| ------------ | -------- | -------------------------------------------------------------- |
| `id`         | Yes      | Unique identifier, typically PascalCase (e.g., `OrderCreated`) |
| `name`       | Yes      | Human-readable name                                            |
| `version`    | Yes      | Semver string (e.g., `0.0.1`)                                  |
| `summary`    | Yes      | 1-2 sentence description of when this event is triggered       |
| `owners`     | Yes      | Array of team or user IDs                                      |
| `schemaPath` | No       | Path to schema file (e.g., `schema.json`, `schema.avro`)       |
| `badges`     | No       | Array of badge objects                                         |
| `draft`      | No       | Object with `title` and `message` for draft events             |

## Schema Files

Schema files sit alongside the `index.mdx` file in the same folder. Supported formats:

- `schema.json` — JSON Schema
- `schema.avro` — Avro schema
- `schema.yml` — YAML schema
- `schema.proto` — Protobuf

Reference schemas in the body with `<Schema />` or `<SchemaViewer />` components.

## Example 1: Event with Schema and Code Samples

````mdx
---
id: InventoryAdjusted
name: Inventory adjusted
version: 1.0.1
summary: |
  Indicates a change in inventory level
owners:
  - dboyne
  - full-stack
badges:
  - content: Recently updated!
    backgroundColor: green
    textColor: green
  - content: "Broker:Apache Kafka"
    backgroundColor: yellow
    textColor: yellow
    icon: kafka
schemaPath: schema.avro
---

## Overview

The `Inventory Adjusted` event is triggered whenever there is a change in the inventory levels of a product. This could occur due to various reasons such as receiving new stock, sales, returns, or manual adjustments by the inventory management team.

## Architecture diagram

<NodeGraph />

<SchemaViewer file="schema.yml" title="JSON Schema" maxHeight="500" />

## Payload example

```json
{
  "event_id": "abc123",
  "timestamp": "2024-01-15T10:30:00Z",
  "product_id": "prod987",
  "adjusted_quantity": 10,
  "new_quantity": 150,
  "adjustment_reason": "restock",
  "adjusted_by": "user123"
}
```
````

## Schema (avro)

<SchemaViewer file="schema.avro" title="Avro schema" maxHeight="500" showRequired="true" />

<Schema file="schema.avro" title="Inventory Adjusted Schema (avro)" />

## Producing the Event

```python
from kafka import KafkaProducer
import json

producer = KafkaProducer(
    bootstrap_servers=['localhost:9092'],
    value_serializer=lambda v: json.dumps(v).encode('utf-8')
)

event_data = {
    "event_id": "abc123",
    "timestamp": "2024-01-15T10:30:00Z",
    "product_id": "prod987",
    "adjusted_quantity": 10,
    "new_quantity": 150,
    "adjustment_reason": "restock",
    "adjusted_by": "user123"
}

producer.send('inventory.adjusted', event_data)
producer.flush()
```

### Consuming the Event

```python
from kafka import KafkaConsumer
import json

consumer = KafkaConsumer(
    'inventory.adjusted',
    bootstrap_servers=['localhost:9092'],
    auto_offset_reset='earliest',
    group_id='inventory_group',
    value_serializer=lambda v: json.dumps(v).encode('utf-8')
)

for message in consumer:
    event_data = json.loads(message.value)
    print(f"Received Inventory Adjusted event: {event_data}")
```

````

## Example 2: Simple Event

```mdx
---
id: OrderCancelled
name: Order Cancelled
version: 0.0.1
summary: |
  Triggered when a customer or system cancels an existing order.
owners:
  - order-management
badges:
  - content: 'Broker:Apache Kafka'
    backgroundColor: yellow
    textColor: yellow
schemaPath: schema.json
---

## Overview

The OrderCancelled event is published when an order is cancelled, whether by customer request, payment failure, or fraud detection. Downstream services use this to release reserved inventory, process refunds, and update dashboards.

<NodeGraph />

## Payload example

```json
{
  "orderId": "ord-12345",
  "customerId": "cust-789",
  "reason": "customer_request",
  "cancelledAt": "2024-01-15T10:30:00Z"
}
````

## Schema

<SchemaViewer file="schema.json" title="JSON Schema" maxHeight="500" />
```

## Key Conventions

- Use `<NodeGraph />` for architecture visualization showing producers and consumers
- Use `<SchemaViewer />` for interactive schema browsing (supports `maxHeight`, `showRequired`, `expand`, `search`)
- Use `<Schema />` for raw schema display
- Include payload examples as JSON code blocks
- Include producer/consumer code samples when helpful
- Use `[[service|ServiceName]]` to link to related services in the body
