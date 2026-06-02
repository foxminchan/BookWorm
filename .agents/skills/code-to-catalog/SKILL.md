---
name: code-to-catalog
description: Turns a codebase into EventCatalog documentation through an evidence-based interview. Scans the code first, proposes an architectural model (domains, services, agents, messages, channels), grills the user on the structural decisions, produces a reviewable plan file, then hands off to catalog-documentation-creator. Use when user says "document my codebase in EventCatalog", "turn this repo into a catalog", "model my code as a catalog", "document my agents", "document my AI agents", "grill me on my architecture", "update my catalog from the code", "reconcile my catalog with my code", or "I don't know where to start documenting this codebase". Works for brand-new catalogs AND for updating existing catalogs that have drifted from the code.
license: MIT
metadata:
  author: eventcatalog
  version: "1.0.0"
---

# Code to Catalog

Turn a codebase into EventCatalog documentation through a guided, evidence-based interview. Works for two situations:

1. **No catalog yet** — document an unfamiliar or undocumented codebase from scratch.
2. **Existing catalog** — reconcile the catalog with the current code (add new resources, flag drift, surface stale entries).

This skill does not write catalog files itself. It produces a **plan file** (`.catalog-plan.md`) that captures the agreed architectural model, then hands off to the `catalog-documentation-creator` skill to generate the actual documentation.

## How this skill works

The skill runs in six phases. Follow them in order — later phases depend on earlier ones.

1. **Locate & inventory** — find the code directory and any existing catalog
2. **Discovery scan** — read the code, form a hypothesis
3. **Reconcile with existing catalog** — categorize findings as `new` / `update` / `unchanged` / `investigate`
4. **Tiered grilling** — interview the user on structural decisions only
5. **Produce the plan file** — write `.catalog-plan.md`, get approval
6. **Handoff** — ask whether to generate the catalog now or stop at the plan

## Conversational style (applies throughout)

- **One question at a time.** Never batch questions. The user answers, then move on.
- **Always provide a recommended answer.** Every question includes what you think is true, with evidence (file path + line number). The user confirms, corrects, or overrides.
- **Cite the code.** When you present a finding, point at the file where you saw it — e.g., `src/orders/events.ts:42`. The user should be able to verify without trusting you.
- **Be honest about uncertainty.** If the code does not tell you whether something is an event or a command, say so. Do not guess silently.
- **Surface conflicts, do not pick silently.** When catalog and code disagree, the user decides. Never overwrite without confirmation.
- **Respect the user's time.** Grilling is tiered on purpose — structural decisions only. Do not grill on per-resource fields (summaries, owners, schemas).
- **No catalog deletions.** Resources in the catalog that you cannot find in code are flagged `investigate` — never removed automatically.

## Phase 1: Locate & inventory

### Find the codebase

Ask the user: **"Which code directory should I analyze?"**

Verify the directory exists and looks like a code project (has a `package.json`, `pom.xml`, `go.mod`, `Cargo.toml`, `pyproject.toml`, source directories, etc.). If the directory is ambiguous (e.g., a monorepo), confirm the scope: the whole repo, or a specific subdirectory.

### Find the catalog

Ask the user: **"Do you already have an EventCatalog project, or do you want to start fresh?"**

**If they already have one:**

- Ask for the path. Verify it's an EventCatalog project by checking for `eventcatalog.config.js` or the standard directories (`services/`, `agents/`, `events/`, `commands/`, `queries/`, `domains/`, `channels/`, `flows/`).
- Build an inventory of what already exists. If the EventCatalog MCP server is connected, use `getResources`, `getResource`, `findResourcesByOwner`. Otherwise read the filesystem directly and parse the frontmatter of each `index.md`/`index.mdx`.
- Record for each resource: `id`, `name`, `version`, `type`, `summary`, and (for services and agents) `sends` / `receives` relationships.
- Note the catalog's conventions: nested (`domains/X/services/Y/events/Z`) vs flat, PascalCase vs kebab-case IDs, existing owners, schema formats in use.

**If they do not have a catalog:**

- That's fine. Note that scaffolding will happen at handoff time through `catalog-documentation-creator` (which runs `npx @eventcatalog/create-eventcatalog@latest <name> --empty`).
- Phase 3 (reconciliation) becomes a no-op — everything discovered will be `new`.

## Phase 2: Discovery scan

Read the codebase and form a hypothesis. Do **not** show the user your findings yet — you'll present them as questions in Phase 4, backed by evidence.

For detailed detection heuristics per language/framework (Node.js, Python, Go, Java, .NET), see `references/discovery.md`. Read that file now if the codebase uses a stack you need guidance on.

Detect:

### Project structure

- Monorepo vs single service (look for workspace configs: `pnpm-workspace.yaml`, `package.json` with `workspaces`, `nx.json`, `turbo.json`, `lerna.json`, multiple top-level service directories with their own manifests).
- Language and framework per service or agent.
- Build/deploy units (Dockerfiles, Helm charts, `serverless.yml`, `cdk` stacks, k8s manifests).

### Service boundaries

A service is an independently-deployable, independently-ownable unit. Signals:

- Separate package with its own manifest
- Separate Dockerfile / deployment config
- Its own entrypoint (`main.ts`, `main.go`, `app.py`, etc.)
- Consumed by others over a network boundary (HTTP, message bus)

When in doubt, mark as a **candidate** and grill the user in Phase 4.

### Agent boundaries

An agent is an AI/LLM-powered runtime or worker that reasons, calls tools, or automates decisions. Signals:

- Explicit names: `*Agent`, `*Assistant`, `*Copilot`, `*Worker` with LLM/tool orchestration
- LLM SDK usage: OpenAI, Anthropic, Gemini, Vercel AI SDK, LangChain, LlamaIndex, Mastra, CrewAI, AutoGen
- Tool registries or callable tools: MCP clients/servers, `tools`, `function_call`, `tool_choice`, `executeTool`
- Agent frameworks: `Agent`, `createAgent`, `runAgent`, graph/workflow nodes using an LLM
- Memory/state stores used by the agent: vector DBs, Redis, Postgres, Supabase, Pinecone, Qdrant, Chroma

Do not classify a plain service as an agent just because it calls an LLM once. Treat it as an agent when the code owns a durable assistant/worker boundary, tool set, model policy, memory, or autonomous workflow.

### Messages (events, commands, queries)

Candidates come from:

- **Naming patterns** — `*Created`, `*Placed`, `*Updated`, `*Deleted` (likely events); `Place*`, `Create*`, `Cancel*`, `Process*` (likely commands); `Get*`, `Find*`, `List*` (likely queries).
- **Message bus clients** — Kafka (`kafkajs`, `confluent-kafka`, `sarama`), RabbitMQ (`amqplib`, `pika`), NATS, AWS SNS/SQS/EventBridge, GCP PubSub, Azure Service Bus.
- **Schema files** — JSON Schema (`.schema.json`), Avro (`.avsc`), Protobuf (`.proto`). These are strong signals of a message contract.
- **DTO / type definitions** — especially if they look like payloads (flat, data-only, named after a domain event).

Classify each candidate as **event**, **command**, or **query** based on evidence:

- Event: past tense, published to a topic/exchange, multiple consumers possible, no direct reply expected.
- Command: imperative, sent to one specific handler, expects to be processed.
- Query: read-only, expects a response.

If evidence is ambiguous (common), mark it as **uncertain** and grill in Phase 4 — do not silently pick.

### Channels

Anywhere messages flow through named infrastructure:

- Kafka topics (string literals passed to `producer.send({ topic: '...' })`)
- RabbitMQ queues/exchanges
- SNS topics, SQS queues
- HTTP endpoints for query services (`GET /users/:id`)

### Domains (candidates)

Strong signals:

- Top-level folder grouping (`src/orders/`, `src/payments/`, `src/shipping/`)
- Bounded-context hints in READMEs, module docs
- Package namespaces (`com.company.orders.*`)
- Ownership files (`CODEOWNERS`, `.codeowners`)

Do not guess domains with low confidence. If unclear, propose "single domain = whole codebase" and let the user split in Phase 4.

### Containers

Databases, caches, queues referenced in config, env vars, or client instantiation:

- Postgres / MySQL / SQLite / Mongo / DynamoDB / Cassandra
- Redis / Memcached
- S3 buckets, GCS buckets

### Output of Phase 2

An internal draft map:

```
domains:
  - name: <candidate>
    confidence: high|medium|low
    services: [...]
    agents: [...]
services:
  - name: <candidate>
    path: <dir>
    sends: [...]
    receives: [...]
    channels: [...]
    containers: [...]
agents:
  - name: <candidate>
    path: <dir>
    model: <provider/name/version if found>
    tools: [...]
    sends: [...]
    receives: [...]
    channels: [...]
    containers: [...]
    flows: [...]
messages:
  - name: <candidate>
    classification: event|command|query|uncertain
    evidence: <file:line>
    producer: <service or agent>
    consumer: <service, agent, or unknown>
channels: [...]
containers: [...]
```

Hold this map internally. You'll use it to drive Phase 3 and Phase 4.

## Phase 3: Reconcile with existing catalog

Skip this phase if there is no existing catalog — everything discovered is `new`.

For each item in the draft map, find the matching resource in the catalog inventory (match by ID, then by name, then by fuzzy match on name + type). Assign a status:

| Status        | Meaning                                                                                                                                         |
| ------------- | ----------------------------------------------------------------------------------------------------------------------------------------------- |
| `unchanged`   | Resource exists in catalog and matches what's in code. No user grilling needed.                                                                 |
| `update`      | Resource exists in catalog, but the code has drifted — new messages emitted, renamed fields, changed schemas, new sends/receives relationships. |
| `new`         | Found in code, not in catalog. Candidate for a new resource.                                                                                    |
| `investigate` | Exists in catalog, not found in code. Possibly stale, possibly removed, possibly in a different repo. **Never delete — only surface.**          |

For `update` items, capture **what specifically drifted**:

- `OrderService` in catalog sends `[OrderPlaced]`, code also sends `OrderCancelled` → drift: new send.
- `PaymentService` schema field renamed `amount_cents` → `amountCents` → drift: schema change.
- `OrderSupportAgent` in catalog has no tools, code defines `orderLookup` and `zendeskNotes` MCP tools → drift: new tools.

This categorization drives Phase 4 — you only grill on `update`, ambiguous `new`, and `investigate` items. `unchanged` resources are silent.

## Phase 4: Tiered grilling

**Grill only on structural decisions.** Per-resource details (summary text, owner names, schema fields, badge styles) are not your concern — `catalog-documentation-creator` handles them.

For the full question bank with recommended-answer templates, see `references/grilling.md`. Read it now.

Walk the topics in this order. Dependencies flow downward — resolve earlier topics before later ones.

### Topic 1: Domains & boundaries

Ask about domain groupings first, because service placement depends on it.

- If you detected clear domain candidates: present each with its recommended services and agents, then ask the user to confirm.
- If you detected none: propose "single domain for the whole codebase" and ask whether they want to split.
- For any ambiguous service or agent ("does `OrderSupportAgent` belong to `Orders` or its own `Support` domain?"), grill it.

### Topic 2: Service boundaries

- Confirm each service candidate. Present its evidence (path, entrypoint, Dockerfile).
- For ambiguous cases ("this module could be its own service or part of another"), grill with a recommended answer.
- Handle monorepo edge cases: is the shared `lib/` package a service? (Probably not — it's infrastructure.)

### Topic 3: Agent boundaries

- Confirm each agent candidate. Present its evidence (LLM SDK usage, model configuration, tool registry, MCP tools, memory store, entrypoint).
- For ambiguous cases ("this service calls an LLM once; should it be a service with an LLM integration or a first-class agent?"), grill with a recommended answer.
- Capture model/provider and tool names as evidence, but avoid grilling on every prompt or low-level tool parameter.

### Topic 4: Message classification

This is the most commonly-wrong call. Grill it hard.

For every message marked `uncertain` in discovery:

> I found `OrderReceived` at `src/orders/handlers.ts:18`. It's consumed from a queue and there's no reply path, so I'd classify it as an **event**. Agree?

For messages with high-confidence classification, present them for quick bulk confirmation:

> I identified these as events (based on past-tense naming + pub/sub pattern): `OrderPlaced`, `OrderCancelled`, `PaymentProcessed`. Any you'd reclassify?

### Topic 5: Drift reconciliation _(only if existing catalog)_

For each `update` item:

> Catalog says `PaymentService` sends `[PaymentProcessed]`. Code also emits `PaymentRefunded` at `src/payments/refund.ts:24`. Add `PaymentRefunded` to the service's sends? I'd say yes.

For each `investigate` item:

> Catalog has `LegacyPaymentConfirmed` (event, v0.0.3). I could not find it in the code. It might live in another repo, or it might be removed. I'll flag it as `investigate` in the plan — the user can decide later. OK?

Do not propose deletions. Only surface.

### What NOT to grill on

- Summary text for each resource
- Owner / team assignments (unless discovered from `CODEOWNERS` and ambiguous)
- Prompt wording and low-level tool parameters for agents
- Schema field-level detail
- Badges, visual customizations
- Flow diagrams (that's `flow-wizard`)

Those pass through to `catalog-documentation-creator` with sensible defaults.

### Watch for interview fatigue

If the grilling is running long (say, more than ~15 questions):

- Summarize progress: "Here's what we've agreed so far — domains X, Y; services A, B, C; 12 messages classified."
- Offer to pause at the plan file: "We can stop here, write the plan, and pick up details later."

## Phase 5: Produce the plan file

Write the plan to `.catalog-plan.md`. Default location: root of the code directory. Ask the user if they'd like it elsewhere.

Use this exact structure:

```markdown
# Catalog Plan

**Generated:** YYYY-MM-DD
**Codebase:** /absolute/path/to/repo
**Existing catalog:** /absolute/path/to/catalog (or "none — will be created")

## Summary

<1–2 paragraph narrative of what was found and the agreed architectural model>

## Domains

- **Orders** (status: new)
  - Services: OrderService, ShippingService
  - Agents: OrderSupportAgent
  - Rationale: both deal with the order lifecycle; confirmed with user

- **Payments** (status: unchanged)
  - Services: PaymentService

## Services

### OrderService (status: new)

- Domain: Orders
- Path in code: /services/orders
- Receives: PlaceOrder (command)
- Sends: OrderPlaced (event), OrderCancelled (event)
- Channels: orders.commands, orders.events
- Containers: orders-db (postgres)

### PaymentService (status: update)

- Domain: Payments
- Path in code: /services/payments
- Drift: code emits new PaymentRefunded event not in catalog
- Sends (after update): PaymentProcessed, PaymentRefunded

## Agents

### OrderSupportAgent (status: new)

- Domain: Orders
- Path in code: /agents/order-support
- Model: OpenAI / gpt-4.1-mini
- Tools: order-lookup (mcp), support-case-notes (mcp)
- Receives: OrderConfirmed, OrderCancelled
- Reads from: orders-db
- Flows: PlaceOrderFlow

### FraudReviewAgent (status: update)

- Domain: Payments
- Path in code: /agents/fraud-review
- Drift: code added fraud-case-queue MCP tool not in catalog
- Receives (after update): PaymentInitiated, RiskScoreCalculated, FraudDetected

## Messages

- **OrderPlaced** (event, status: new) — emitted by OrderService via orders.events
- **OrderCancelled** (event, status: new) — emitted by OrderService via orders.events
- **PlaceOrder** (command, status: new) — handled by OrderService
- **PaymentRefunded** (event, status: new) — emitted by PaymentService
- **LegacyPaymentConfirmed** (event, status: investigate) — in catalog, not found in code

## Channels

- orders.events (Kafka topic, status: new)
- orders.commands (Kafka topic, status: new)

## Containers

- orders-db (postgres, status: new) — used by OrderService

## Open decisions / rationale

- Classified `CancelOrder` as a command (user confirmed — it expects a handler)
- Kept Shipping as a sub-area of Orders rather than splitting (user preference)
- Flagged `LegacyPaymentConfirmed` for manual review — may live in a different repo

## Next step

Run `catalog-documentation-creator` with this plan to generate resources marked `new` or `update`.
```

**Status values** (use exactly these): `new`, `update`, `unchanged`, `investigate`.

After writing, show the plan to the user and ask for explicit approval: **"Here's the plan. Does this match what we agreed? Anything to add, change, or remove before we proceed?"**

Loop on edits until the user approves.

## Phase 6: Handoff

Once the plan is approved, ask:

> **"Generate the catalog now, or stop here with just the plan?"**

**If generate now:**

- Invoke the `catalog-documentation-creator` skill, passing the plan file path.
- Tell it to only create/update resources flagged `new` or `update`. Skip `unchanged`. Report `investigate` items to the user as a list — do not auto-handle.
- If the user has no existing catalog, `catalog-documentation-creator` will scaffold one first.

**If stop here:**

- Confirm the plan file location.
- Tell the user how to resume: "When you're ready, run `catalog-documentation-creator` and point it at this plan file. It will generate the `new` and `update` resources."

### After handoff

Let the user know:

- Where the plan was saved.
- (If generated) which resources were created/updated.
- Which `investigate` items need manual review.
- That they can run `flow-wizard` next if they want to document business flows across the catalog.

## Quality checklist

Before finishing, verify:

1. The plan file exists at the agreed path.
2. Every domain, service, agent, message, channel, and container has a status: `new` / `update` / `unchanged` / `investigate`.
3. Every `update` item lists what specifically drifted.
4. Every `investigate` item is flagged, not deleted.
5. Message classifications (event / command / query) were either confirmed by the user or clearly recommended with evidence.
6. Service-to-domain and agent-to-domain mapping is explicit for every service and agent.
7. No per-resource grilling happened (summaries, owners, schemas — those are for `catalog-documentation-creator`).
8. If handing off, `catalog-documentation-creator` has the plan path and instructions to skip `unchanged`.
