# Flows

## Format

**File:** `index.mdx` inside a flow folder
**Location:** `flows/{FlowName}/index.mdx` or nested under a resource (e.g Domain) `domains/{Domain}/flows/{FlowName}/index.mdx`, or nested in any directory `flows/Payments/MyPaymentFlow/{FlowName}/index.mdx`

## Frontmatter Fields

| Field     | Required | Description                                    |
| --------- | -------- | ---------------------------------------------- |
| `id`      | Yes      | Unique identifier (e.g., `CancelSubscription`) |
| `name`    | Yes      | Human-readable name                            |
| `version` | Yes      | Semver string (e.g., `0.0.1`)                  |
| `summary` | No       | Description of the business process            |
| `owners`  | No       | Array of team or user IDs                      |
| `steps`   | Yes      | Array of step objects defining the flow        |

## Step Types

Each step has an `id`, `title`, and one of four types:

| Type            | Field                                    | Use When                                        |
| --------------- | ---------------------------------------- | ----------------------------------------------- |
| Actor           | `actor: { name }`                        | A person or external entity initiates an action |
| Message         | `message: { id, version }`               | An event, command, or query is exchanged        |
| Service         | `service: { id, version }`               | A service processes something                   |
| External System | `externalSystem: { name, summary, url }` | A third-party system is involved                |

## Step Transitions

- **Single next step:** `next_step: { id, label }`
- **Branching (multiple paths):** `next_steps: [{ id, label }, { id, label }]`
- **Terminal step:** Omit `next_step` / `next_steps` to end the flow

## Example: Flow with Branching and External Systems

```mdx
---
id: CancelSubscription
name: "User Cancels Subscription"
version: "1.0.0"
summary: "Flow for when a user has cancelled a subscription"
owners:
  - subscriptions-management
steps:
  - id: "cancel_subscription_initiated"
    title: "Cancels Subscription"
    summary: "User cancels their subscription"
    actor:
      name: "User"
    next_step:
      id: "cancel_subscription_request"
      label: "Initiate subscription cancellation"

  - id: "cancel_subscription_request"
    title: "Cancel Subscription"
    message:
      id: "CancelSubscription"
      version: "0.0.1"
    next_step:
      id: "subscription_service"
      label: "Proceed to subscription service"

  - id: "stripe_integration"
    title: "Stripe"
    externalSystem:
      name: "Stripe"
      summary: "3rd party payment system"
      url: "https://stripe.com/"
    next_step:
      id: "subscription_service"
      label: "Return to subscription service"

  - id: "subscription_service"
    title: "Subscription Service"
    service:
      id: "SubscriptionService"
      version: "0.0.1"
    next_steps:
      - id: "stripe_integration"
        label: "Cancel subscription via Stripe"
      - id: "subscription_cancelled"
        label: "Successful cancellation"
      - id: "subscription_rejected"
        label: "Failed cancellation"

  - id: "subscription_cancelled"
    title: "Subscription has been Cancelled"
    message:
      id: "UserSubscriptionCancelled"
      version: "0.0.1"
    next_step:
      id: "notification_service"
      label: "Email customer"

  - id: "subscription_rejected"
    title: "Subscription cancellation has been rejected"

  - id: "notification_service"
    title: "Notifications Service"
    service:
      id: "NotificationService"
      version: "0.0.2"
---

<NodeGraph />
```

## Key Conventions

- Steps can reference back to earlier steps to create loops (e.g., service calls external system, then external system returns to service)
- Terminal steps have no `next_step` — the flow ends there
- Use `summary` on steps for additional context
- The body is typically just `<NodeGraph />` — the flow visualization is auto-generated from the steps
- Message IDs in steps must match actual event/command/query IDs in the catalog
- Service IDs in steps must match actual service IDs in the catalog
