# Flows

## Format

**File:** `index.mdx` inside a flow folder
**Location:** any `flows/` folder: `flows/{FlowName}/index.mdx`, nested under a domain `domains/{Domain}/flows/{FlowName}/index.mdx`, a system (`systems/{System}/flows/{FlowName}/index.mdx`, `domains/{Domain}/systems/{System}/flows/{FlowName}/index.mdx`), or grouped in folders (`flows/Payments/{FlowName}/index.mdx`)
**Older versions:** `{FlowName}/versioned/{version}/index.mdx`

The folder doesn't attach a flow to a domain or system — add it to their `flows` frontmatter (see Key Conventions).

## Frontmatter Fields

| Field     | Required | Description                                    |
| --------- | -------- | ---------------------------------------------- |
| `id`      | Yes      | Unique identifier (e.g., `CancelSubscription`) |
| `name`    | Yes      | Human-readable name                            |
| `version` | Yes      | Semver string (e.g., `0.0.1`)                  |
| `summary` | No       | Description of the business process            |
| `owners`  | No       | Array of team or user IDs                      |
| `steps`   | Yes      | Array of step objects defining the flow        |

## Steps

Every step has an `id` and points at **at most one** thing. What it points at decides how it is drawn; a step that points at nothing is drawn as a plain titled box.

| Key                        | Shape                                                                           | Use When                                                             |
| -------------------------- | ------------------------------------------------------------------------------- | -------------------------------------------------------------------- |
| `message`                  | `{ id, version? }`                                                              | An event, command, or query is exchanged                             |
| `service`                  | `{ id, version? }`                                                              | A service processes something                                        |
| `agent`                    | `{ id, version? }`                                                              | An AI agent reasons, invokes tools, or automates part of the process |
| `channel`                  | `{ id, version? }`                                                              | Show where a message travels (topic, queue, bus)                     |
| `system` (alias `systems`) | `{ id, version? }`                                                              | A whole system (internal or `scope: external`) takes part            |
| `flow`                     | `{ id, version? }`                                                              | Hand off to another flow (sub-flow)                                  |
| `container`                | `{ id, version? }`                                                              | A data store is read or written                                      |
| `dataProduct`              | `{ id, version? }`                                                              | A data product is produced or consumed                               |
| `actor`                    | `{ name, summary? }`                                                            | A person or role initiates an action                                 |
| `externalSystem`           | `{ name, summary?, url? }`                                                      | A third-party system that isn't documented in the catalog            |
| `custom`                   | `{ title?, icon?, type?, summary?, url?, color?, properties?, height?, menu? }` | Anything else (a manual step, a UI, a cron job)                      |

Other step fields:

- `title` — optional. Defaults to the name of what the step points at (or the actor / external system / custom node's name, or the step's `id`). Set it when the default isn't descriptive enough.
- `summary` — optional extra context shown on the step.
- Omit `version` on a pointer to use the latest version.
- Don't set `type` on steps — it's accepted for older flows but ignored.

## Step Transitions

- **Single next step:** `next_step: { id, label }`, or just the step id: `next_step: subscription_service`
- **Branching (multiple paths):** `next_steps: [{ id, label }, { id, label }]` (entries can also be bare ids)
- **Terminal step:** Omit `next_step` / `next_steps` to end the flow
- `label` is optional

## Validation

The build and `eventcatalog-linter` enforce these rules:

- **Unknown keys fail the build** — on the flow and on each step. Custom properties must start with `x-` (e.g. `x-sla: 2h`).
- **A step can only be one thing** — `service` and `message` on the same step fails; split it into two steps.
- **`next_step` and `next_steps` together fail** — use one or the other.
- **References must exist** — a `next_step` to a step that isn't in the flow, or a pointer to a resource that isn't in the catalog, logs a warning in the build but fails the linter (`refs/flow-step-exists`, `refs/resource-exists`). For things that aren't documented yet, use an `actor`, `externalSystem`, `custom` or plain titled step instead of a pointer.

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
    next_step: "subscription_service"

  - id: "subscription_service"
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
    next_step:
      id: "recovery_agent"
      label: "Ask agent to suggest recovery path"

  - id: "notification_service"
    service:
      id: "NotificationService"

  - id: "recovery_agent"
    title: "Subscription Recovery Agent"
    agent:
      id: "SubscriptionRecoveryAgent"
      version: "0.0.1"
---

<NodeGraph />
```

`title` is left out where the referenced resource's name is good enough (`cancel_subscription_request` shows the `CancelSubscription` message's name). `subscription_rejected` points at nothing, so it's drawn as a plain titled box.

## Grouping

Flows can be grouped by `domain`, `system`, `team` or `domain-system` (domains with their systems nested). Groups come from the catalog (each step's domain, system and owners), so the frontmatter needs nothing extra. Readers can switch grouping from the flow page; to open a flow already grouped, use the `<Flow />` component:

```mdx
<Flow id="CancelSubscription" lanes="team" />
```

Flows don't have L1/L2/L3 levels.

## Key Conventions

- Steps can reference back to earlier steps to create loops (e.g., service calls external system, then external system returns to service)
- Terminal steps have no `next_step` — the flow ends there
- Use `summary` on steps for additional context
- The body is typically just `<NodeGraph />` — the flow visualization is auto-generated from the steps
- Every pointer (`message`, `service`, `agent`, `channel`, `system`, `flow`, `container`, `dataProduct`) must match an id in the catalog
- Add a `channel` step between a producer and a message's consumers when the channel matters to the story
- When a flow belongs to a domain or system, add it to that domain's or system's `flows` frontmatter (`flows: [{ id: CancelSubscription }]`) — folder location alone doesn't link them
