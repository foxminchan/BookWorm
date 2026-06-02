# Agents

## Format

**File:** `index.mdx` inside an agent folder
**Location:** `agents/{AgentName}/index.mdx`, `domains/{Domain}/agents/{AgentName}/index.mdx`, or `domains/{Domain}/subdomains/{Subdomain}/agents/{AgentName}/index.mdx`

Agents document AI agents, copilots, autonomous workers, and LLM-powered assistants that participate in the architecture.

## Frontmatter Fields

| Field        | Required | Description                                                         |
| ------------ | -------- | ------------------------------------------------------------------- |
| `id`         | Yes      | Unique identifier, typically PascalCase (e.g., `OrderSupportAgent`) |
| `name`       | Yes      | Human-readable name                                                 |
| `version`    | Yes      | Semver string (e.g., `0.0.1`)                                       |
| `summary`    | Yes      | 1-2 sentence description of the agent                               |
| `owners`     | Yes      | Array of team or user IDs                                           |
| `model`      | No       | Object with `provider`, `name`, and optional `version`              |
| `tools`      | No       | Array of tools the agent can call                                   |
| `sends`      | No       | Array of events, commands, or queries this agent produces           |
| `receives`   | No       | Array of events, commands, or queries this agent consumes           |
| `writesTo`   | No       | Array of containers/databases the agent writes to                   |
| `readsFrom`  | No       | Array of containers/databases the agent reads from                  |
| `flows`      | No       | Array of business flows this agent participates in                  |
| `repository` | No       | Object with `language` and `url`                                    |
| `badges`     | No       | Array of badge objects                                              |
| `deprecated` | No       | Object with `date` and `message` for deprecated agents              |

## Model and Tools

Use `model` when the provider/model is known:

```yaml
model:
  provider: OpenAI
  name: gpt-4.1-mini
  version: "2025-04-14"
```

Each tool supports:

| Field         | Required | Description                                                           |
| ------------- | -------- | --------------------------------------------------------------------- |
| `name`        | Yes      | Tool display name                                                     |
| `type`        | Yes      | Tool type such as `mcp`, `api`, `database`, `function`, or `workflow` |
| `icon`        | No       | Public icon path, e.g. `/icons/tools/slack.svg`                       |
| `url`         | No       | Tool endpoint or documentation URL                                    |
| `description` | No       | What the tool gives the agent permission or context to do             |

Prefer `type: mcp` for MCP servers. Use `<AgentTools />` in the body when `tools` are defined.

## Channel Routing

Agents use the same message routing shape as services:

```yaml
receives:
  - id: OrderCancelled
    from:
      - id: orders-domain-eventbus
sends:
  - id: SupportCaseUpdated
    to:
      - id: support-events
```

## Example: Agent with Tools, Messages, and Containers

```mdx
---
id: OrderSupportAgent
name: Order Support Agent
version: 0.0.1
summary: Assists support teams by investigating customer order questions and suggesting the next best action.
owners:
  - support-platform
model:
  provider: OpenAI
  name: gpt-4.1-mini
  version: "2025-04-14"
repository:
  language: TypeScript
  url: https://github.com/example/order-support-agent
tools:
  - name: Order lookup
    type: mcp
    icon: /icons/tools/snowflake.svg
    url: https://mcp.example.com/orders/lookup
    description: Retrieves order status, totals, shipment milestones, and recent order events from the operational read model.
  - name: Support case notes
    type: mcp
    icon: /icons/tools/zendesk.svg
    url: https://mcp.example.com/support/case-notes
    description: Appends investigation notes, suggested customer replies, and follow-up actions to the support ticket.
receives:
  - id: OrderConfirmed
  - id: OrderCancelled
readsFrom:
  - id: orders-db
flows:
  - id: PlaceOrderFlow
---

The Order Support Agent helps the support team answer customer order questions without manually checking every backing service. It receives [[event|OrderConfirmed]] and [[event|OrderCancelled]] so support context stays fresh as orders move through the lifecycle.

It belongs to the [[domain|Orders]] domain and works alongside [[service|OrdersService]], [[service|NotificationService]], and [[service|ShippingService]].

## Tools

<AgentTools />

## Architecture

<NodeGraph />

## Responsibilities

- Summarize the current state of an order for support staff.
- Identify likely causes for cancellation, fulfillment, or delivery issues.
- Suggest customer-facing response drafts based on the latest order and notification state.
```

## Domain Membership

When an agent belongs to a domain, add it to the domain's `agents` frontmatter:

```yaml
agents:
  - id: OrderSupportAgent
```

If using nested domains, place the agent under the relevant domain:

```
domains/Orders/agents/OrderSupportAgent/index.mdx
domains/E-Commerce/subdomains/Orders/agents/OrderSupportAgent/index.mdx
```

## Key Conventions

- Use `[[agent|AgentName]]` syntax to link to other agents in the body
- Use `[[service|ServiceName]]`, `[[event|EventName]]`, `[[domain|DomainName]]`, and `[[container|ContainerName]]` for related resources
- Include `<AgentTools />` when `tools` are defined
- Include `<NodeGraph />` for architecture visualization
- Use `readsFrom`/`writesTo` for data stores and memory stores, not tools
- Use `tools` for callable APIs, MCP servers, functions, workflows, or SaaS integrations
- Do not add `specifications`; agent resources do not support service API specifications
