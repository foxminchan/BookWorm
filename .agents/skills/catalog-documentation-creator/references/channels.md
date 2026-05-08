# Channels

## Format

**File:** `index.mdx` inside a channel folder
**Location:** `channels/{ChannelName}/index.mdx` or nested under a service `services/{Service}/channels/{ChannelName}/index.mdx`

## Frontmatter Fields

| Field               | Required | Description                                                                            |
| ------------------- | -------- | -------------------------------------------------------------------------------------- |
| `id`                | Yes      | Unique identifier (e.g., `orders.{env}.events`, `inventory-domain-eventbus`)           |
| `name`              | Yes      | Human-readable name                                                                    |
| `version`           | Yes      | Semver string (e.g., `0.0.1`)                                                          |
| `summary`           | Yes      | Description of the channel's purpose                                                   |
| `owners`            | Yes      | Array of team or user IDs                                                              |
| `address`           | No       | The actual address/topic name                                                          |
| `protocols`         | No       | Array of protocol strings (e.g., `kafka`, `azure-servicebus`, `aws-cross-account-bus`) |
| `deliveryGuarantee` | No       | `at-least-once`, `at-most-once`, or `exactly-once`                                     |
| `routes`            | No       | Array of channel IDs this channel routes messages to                                   |
| `parameters`        | No       | Object defining parameterized parts of the channel address                             |

## Channel Routing

Channels can route messages to other channels using the `routes` field. This models message flow through infrastructure:

```
orders-domain-eventbus → cross-account-bus → shipping-domain-eventbus
```

```yaml
# orders-domain-eventbus routes to cross-account-bus
routes:
  - id: cross-account-bus

# cross-account-bus routes to downstream buses
routes:
  - id: registrations-domain-eventbus
  - id: shipping-domain-eventbus
```

## How Services Connect to Channels

Services use `to` and `from` fields in their `sends`/`receives` arrays (see `references/services.md`):

```yaml
# Service sends OrderConfirmed TO a channel
sends:
  - id: OrderConfirmed
    to:
      - id: orders-domain-eventbus

# Service receives OrderConfirmed FROM a channel
receives:
  - id: OrderConfirmed
    from:
      - id: orders-domain-eventbus
```

This creates the full picture: Service A → Channel → Service B

## Example 1: Channel with Routing and Parameters

````mdx
---
id: orders.{env}.events
name: Order Events Channel
version: 1.0.1
summary: |
  Central event stream for all order-related events in the order processing lifecycle
owners:
  - dboyne
address: orders.{env}.events
protocols:
  - azure-servicebus
deliveryGuarantee: at-least-once
routes:
  - id: sns-channel
parameters:
  env:
    enum:
      - dev
      - sit
      - prod
    description: "Environment to use"
---

### Overview

The Orders Events channel is the central stream for all order-related events across the order processing lifecycle. This includes order creation, updates, payment status, fulfillment status, and customer communications.

<ChannelInformation />

### Publishing a message using Azure Service Bus

```python
from azure.servicebus import ServiceBusClient, ServiceBusMessage

CONNECTION_STR = "YOUR_AZURE_SERVICE_BUS_CONNECTION_STRING"
QUEUE_NAME = "orders"

order_event_data = {
    "eventType": "ORDER_CREATED",
    "timestamp": "2024-01-15T10:30:00Z",
    "payload": {
        "orderId": "12345",
        "customerId": "CUST-789"
    }
}

with ServiceBusClient.from_connection_string(conn_str=CONNECTION_STR) as client:
    sender = client.get_queue_sender(queue_name=QUEUE_NAME)
    with sender:
        message = ServiceBusMessage(json.dumps(order_event_data))
        sender.send_messages(message)
```
````

````

## Example 2: Event Bus with Cross-Account Routing

```mdx
---
id: cross-account-bus
name: Organization Cross Account Bus
version: 1.0.0
summary: |
  Amazon Organization Cross Account Bus
owners:
  - dboyne
address: cross-account-bus.amazonaws.com
protocols:
  - aws-cross-account-bus
deliveryGuarantee: at-least-once
parameters:
  accountId:
    description: 'The AWS account ID to use'
routes:
  - id: registrations-domain-eventbus
  - id: shipping-domain-eventbus
---

## Overview

The cross-account event bus enables event routing between AWS accounts in the organization.

<ChannelInformation />
````

## Example 3: Domain-Scoped Event Bus

```mdx
---
id: inventory-domain-eventbus
name: Inventory Domain EventBus
version: 1.0.0
summary: |
  The event bus for the Inventory domain. It is used to send events to and from the Inventory domain. Events can be consumed from this bus by other domains.
owners:
  - dboyne
deliveryGuarantee: at-least-once
routes:
  - id: cross-account-bus
---

<ChannelInformation />
```

## Key Conventions

- Use `<ChannelInformation />` component to display channel metadata (protocols, delivery guarantee, parameters)
- Channel IDs can contain dots and braces for parameterized addresses (e.g., `orders.{env}.events`)
- `routes` creates directed edges in the architecture visualizer between channels
- Channels can be nested under services when they are service-specific (e.g., a service's SQS queue)
- Include code examples showing how to publish/consume from the channel
