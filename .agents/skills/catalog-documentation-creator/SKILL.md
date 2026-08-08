---
name: catalog-documentation-creator
description: Generates EventCatalog documentation files (systems, services, agents, events, commands, queries, domains, flows, channels, containers, ADRs, data products, entities, diagrams) with correct frontmatter, folder structure, and best practices. Use when user asks to "document a system", "document a service", "document an agent", "document an AI agent", "create EventCatalog files", "add an event to the catalog", "document my architecture", "generate catalog documentation", "create documentation for my microservice", "document a database", "create an ADR", "document a data product", or "document an entity".
license: MIT
metadata:
  author: eventcatalog
  version: "1.0.0"
---

# EventCatalog Documentation Creator

Generate properly formatted EventCatalog documentation files following project conventions and best practices.

## Instructions

### Step 1: Locate or Create the User's Catalog

Before generating any files, ask the user: **"Do you already have an EventCatalog project, or would you like to create a new one?"**

**If they already have a catalog:**

- Ask: **"Where is your EventCatalog project?"** — It could be:
  - A repo they've cloned locally (e.g., `~/projects/my-catalog/`)
  - A folder on their machine
  - A monorepo with the catalog in a subdirectory
- Verify it looks like an EventCatalog project by checking for an `eventcatalog.config.js` file or known directories (`systems/`, `services/`, `agents/`, `events/`, `domains/`, `adrs/`, `data-products/`, `entities/`, etc.)
- Read the existing structure to understand whether they use **nested** (domains/services/agents/events) or **flat** (top-level services/, agents/, events/) organization

**If they don't have a catalog yet:**

- Ask where they'd like to create it (default: current directory)
- Run the following command to scaffold a new empty catalog:
  ```bash
  npx @eventcatalog/create-eventcatalog@latest my-catalog --empty
  ```
  (Replace `my-catalog` with the user's preferred name)
- This creates a ready-to-use EventCatalog project with the correct structure
- All generated documentation files go inside this new catalog directory

CRITICAL: All generated files must be written to the user's catalog directory, not just displayed. Always ask where they want resources documented — never assume.

### Step 2: Understand What the User Wants to Document

Ask the user what they want to document. Common scenarios:

- A single service or agent and its messages
- A system that groups services, containers, flows, entities, actors, and related systems
- An event, command, or query
- A full domain with nested services
- A business flow across services and agents
- A channel (Kafka topic, RabbitMQ queue, etc.)
- A container (database, cache, queue)
- An architecture decision record (ADR)
- A data product for analytics, reporting, ML features, or operational data outputs
- A domain entity or aggregate
- A reusable diagram resource

Gather this information before generating:

- Resource name and purpose
- Version (default to `0.0.1` for new resources)
- System boundary, scope (`internal` or `external`), actors, relationships, and contained resources when documenting systems
- Message relationships (what it sends/receives)
- Channel routing (what channels messages flow through)
- Containers (what databases/caches the service reads from or writes to)
- ADR links (what resources a decision applies to, and whether it supersedes/amends another ADR)
- Data product lineage (inputs, outputs, contracts, freshness/SLA expectations)
- Entities and relationships (identifier, properties, references, aggregate root)
- Diagram notation (Mermaid, PlantUML, or other supported fenced diagram formats)
- Agent model/provider and tools when documenting agents
- Schema format if applicable (JSON Schema, Avro, Protobuf)

If the user points you at a codebase (not the catalog), analyze it to extract services, agents, messages, schemas, and relationships — then generate the corresponding catalog documentation.

### Step 3: Check the Existing Catalog

If the catalog directory already has resources, read the existing files to understand:

- Naming conventions (PascalCase IDs? kebab-case?)
- Folder structure (nested under domains or flat?)
- Which owners/teams are already defined
- Badge styles and patterns used
- Schema formats in use (JSON Schema, Avro, etc.)

Match new documentation to these existing conventions.

If the user has the EventCatalog MCP server connected:

1. Use `getResources` to see what already exists in the catalog
2. Use `getResource` to check conventions used in existing entries (naming patterns, owner formats, badge styles)
3. Use `findResourcesByOwner` to suggest consistent ownership
4. Use `getSchemaForResource` to match existing schema formats

This ensures new documentation is consistent with what's already in the catalog.

### Step 4: Generate the Documentation

Generate files following the resource-specific references. Consult the appropriate reference file for the resource type:

- `references/services.md` — Services with sends/receives, channel routing, containers
- `references/systems.md` — Systems with scope, services, containers, flows, entities, actors, and system relationships
- `references/agents.md` — Agents with model metadata, tools, sends/receives, containers, and flows
- `references/events.md` — Events with schemas, payload examples, producer/consumer code
- `references/commands.md` — Commands with REST operations and schemas
- `references/queries.md` — Queries with REST operations and response schemas
- `references/domains.md` — Domains with subdomains, services, and business context
- `references/flows.md` — Business flows with steps, branching, and external systems
- `references/channels.md` — Channels with routing, protocols, and parameters
- `references/containers.md` — Containers (databases, caches, queues) with data classification
- `references/adrs.md` — Architecture decision records with status, date, decision makers, appliesTo, and relationships
- `references/data-products.md` — Data products with inputs, outputs, data contracts, lineage, and SLAs
- `references/entities.md` — DDD/domain entities with identifiers, properties, relationships, and aggregate roots
- `references/diagrams.md` — Reusable diagram resources (Mermaid, PlantUML, architecture diagrams)
- `references/ubiquitous-language.md` — Ubiquitous language terms per domain (DDD glossary/dictionary)
- `references/teams-and-users.md` — Teams and users (ownership)
- `references/components.md` — Components (NodeGraph, Schema, Mermaid, Tabs, etc.) and resource references (`[[type|Name]]` wiki-style links)
- `references/supporting-collections.md` — Changelogs, resource docs, custom docs, schemas, and Studio designs

Every resource file MUST include:

- Valid YAML frontmatter between `---` delimiters
- `id` field matching existing catalog conventions
- `name` as human-readable display name
- `version` as semantic version string
- `summary` as a concise 1-2 sentence description

CRITICAL: Always use `index.mdx` as the filename for versioned resources (systems, services, agents, events, commands, queries, domains, flows, channels, containers, ADRs, data products, entities, diagrams). Teams and users use `{id}.mdx` files directly. Changelogs use `changelog.mdx` or `changelog.md`. Ubiquitous language uses `ubiquitous-language.mdx`. Place files in the correct folder path following the nested structure pattern:

```
domains/{DomainName}/systems/{SystemName}/index.mdx
domains/{DomainName}/systems/{SystemName}/services/{ServiceName}/index.mdx
domains/{DomainName}/systems/{SystemName}/containers/{ContainerName}/index.mdx
domains/{DomainName}/services/{ServiceName}/events/{EventName}/index.mdx
domains/{DomainName}/agents/{AgentName}/index.mdx
domains/{DomainName}/data-products/{DataProductName}/index.mdx
domains/{DomainName}/entities/{EntityName}/index.mdx
domains/{DomainName}/diagrams/{DiagramName}/index.mdx
```

Or flat structure if the catalog uses that pattern:

```
systems/{SystemName}/index.mdx
services/{ServiceName}/index.mdx
agents/{AgentName}/index.mdx
events/{EventName}/index.mdx
adrs/{adr-id}/index.mdx
data-products/{DataProductName}/index.mdx
entities/{EntityName}/index.mdx
diagrams/{DiagramName}/index.mdx
```

Do not generate `schemas` collection entries directly. Generate or reference schema files from events, commands, or queries using `schemaPath` or `schemas`; EventCatalog creates the `schemas` collection from those references. Do not hand-author `designs` unless the user explicitly provides `.ecstudio` content from EventCatalog Studio.

### Step 5: Validate the Output

Before presenting the files to the user, verify:

- YAML frontmatter has `---` delimiters on both sides
- All `id` fields are consistent (no spaces, match folder name)
- All `version` fields are valid semver strings (e.g., `0.0.1`)
- All message references in `sends`/`receives` include `id` and optionally `version`
- System `services`, `containers`, `flows`, `entities`, `relationships`, and domain `systems` references include `id` and optionally `version`
- System `scope` is either `internal` or `external`, and actor `direction` is either `inbound` or `outbound`
- Channel routing uses `to`/`from` fields correctly in sends/receives
- Schema files referenced in `schemaPath` actually exist or are generated
- `<NodeGraph />` component is included for architecture visualization
- Owner IDs reference real teams/users in the catalog

## Common Patterns

### Documenting a Service That Processes Messages

When a user says "document my payment service that receives OrderCreated events and sends PaymentProcessed events":

1. Generate the service `index.mdx` with `receives` and `sends` arrays
2. If messages flow through channels, add `to`/`from` fields to the sends/receives
3. Generate each event `index.mdx` if they don't already exist in the catalog
4. Include `<NodeGraph />` in the service body to show message flow
5. Generate related entities if the service owns important domain objects
6. Add example payload sections for each message
7. Place files in the correct nested folder structure

### Documenting a System

When a user describes a capability, subsystem, product capability, or external system:

1. Decide whether the system belongs inside a domain (`domains/{Domain}/systems/{System}/index.mdx`) or should live at the catalog root (`systems/{System}/index.mdx`) because it is shared, external, or not owned by one domain
2. Generate the system `index.mdx` with `scope`, `owners`, and references to its `services`, `containers`, `flows`, `entities`, and `diagrams` where known
3. Add `relationships` for one-directional links to other systems, using a short `label` for the edge
4. Add `actors` for people, roles, or external participants, using `direction: inbound` when the actor interacts with the system and `direction: outbound` when the system reaches out to the actor
5. Include `<ContextDiagram />` to show actors and system-to-system relationships
6. Include `<NodeGraph />` to show the resources inside the system
7. Generate nested resources under the system folder when they are owned by that system, for example `domains/{Domain}/systems/{System}/services/{Service}/index.mdx`
8. If the system is nested under a domain, add it to the domain's `systems` frontmatter. Every system nested inside a domain MUST be referenced in that domain's `index.mdx`:
   ```yaml
   systems:
     - id: cart-system
       version: 1.0.0
     - id: promotion-system
       version: 1.0.0
   ```
9. If ADRs apply to the system boundary, persistence, integration pattern, or ownership model, link them with `appliesTo: [{ type: system, id: ... }]`

### Documenting an Agent

When a user says "document my support agent that reads order data and uses Zendesk":

1. Generate the agent `index.mdx` with `model`, `tools`, `receives`/`sends`, `readsFrom`/`writesTo`, and `flows` where known
2. Generate or reference events/commands/queries the agent consumes or produces
3. Generate containers for data stores the agent reads or writes
4. Include `<AgentTools />` when tools are documented
5. Include `<NodeGraph />` so the agent appears in architecture visualizations
6. If the agent belongs to a domain, add it to the domain's `agents` frontmatter

### Documenting a Domain

CRITICAL: A domain MUST have at least one system, service, or agent. Never create an empty domain. If the user describes a domain, ensure systems, services, or agents are identified and generated for it.

When a user wants to document a full domain:

1. Identify the systems, services, and agents that belong to this domain. If the user hasn't specified any, ask them: "What systems, services, or agents belong to this domain?" Do NOT create an empty domain.
2. Generate the domain `index.mdx` with the `systems` field listing every system, the `services` field listing every direct domain service, and the `agents` field listing every direct domain agent
3. Include `entities`, `data-products`, `flows`, and `diagrams` fields when those resources belong directly to the domain
4. Generate each system, service, and agent within the domain
5. Generate each message referenced by the services and agents
6. Generate entities, data products, diagrams, and channels if the user describes them
7. Use the nested folder structure: `domains/{Domain}/systems/{System}/`, `domains/{Domain}/systems/{System}/services/{Service}/events/{Event}/`, `domains/{Domain}/services/{Service}/events/{Event}/`, `domains/{Domain}/agents/{Agent}/`, `domains/{Domain}/entities/{Entity}/`, and `domains/{Domain}/data-products/{DataProduct}/`
8. Generate a `ubiquitous-language.mdx` file for the domain by extracting domain-specific terms from service names, agent names, event/command names, entities, and business processes. Place it at `domains/{Domain}/ubiquitous-language.mdx`. See `references/ubiquitous-language.md` for format and examples.
9. CRITICAL: After generating all files, verify the domain's frontmatter `systems` field lists every system, `services` lists every direct domain service, and `agents` lists every direct domain agent that belongs to it. Every system, service, or agent created directly under a domain MUST be referenced in the domain's `index.mdx`:
   ```yaml
   systems:
     - id: CheckoutSystem
   services:
     - id: OrdersService
     - id: InventoryService
     - id: PaymentService
   agents:
     - id: OrderSupportAgent
   ```
   If a system, service, or agent is nested inside the domain folder but not listed in the domain's frontmatter, it will not appear as part of that domain. Always cross-check.

### Documenting an ADR

When a user describes an architecture decision:

1. Generate `adrs/{adr-id}/index.mdx`
2. Use one of the supported statuses: `proposed`, `accepted`, `rejected`, `deprecated`, or `superseded`
3. Include a `date` in `YYYY-MM-DD` format
4. Add `decisionMakers` and `owners` using existing team/user IDs where known
5. Use `appliesTo` to link the decision to impacted resources (`system`, `service`, `event`, `domain`, `flow`, `data-product`, `entity`, etc.)
6. Use `supersedes`, `supersededBy`, `amends`, `amendedBy`, or `related` when linking ADRs together
7. Structure the body with `Context`, `Decision`, and `Consequences`

### Documenting a Data Product

When a user describes analytics, reporting, BI, ML feature, or derived operational data:

1. Generate `data-products/{DataProductName}/index.mdx` or nest it under the relevant domain/subdomain
2. Add `inputs` for upstream messages, services, containers, channels, or other resources
3. Add `outputs` for produced messages, services, containers, channels, or contracts
4. If an output has a data contract, include `contract.path`, `contract.name`, and `contract.type`
5. Include `<NodeGraph />` and any relevant `<SchemaViewer />` for contract files
6. Document lineage, freshness, ownership, access patterns, and SLAs

### Documenting an Entity

When a user describes a domain model, aggregate, data object, or business concept with properties:

1. Generate `entities/{EntityName}/index.mdx`, `domains/{Domain}/entities/{EntityName}/index.mdx`, or `services/{Service}/entities/{EntityName}/index.mdx` depending on catalog structure
2. Include `identifier` and `aggregateRoot: true` when applicable
3. Add `properties` with `name`, `type`, `required`, and `description`
4. Use `references`, `referencesIdentifier`, and `relationType` for relationships to other entities
5. Include `<EntityPropertiesTable />` in the body to render the property table
6. Link entities from domain/service frontmatter using `entities`

### Documenting a Diagram

When a user provides or asks for a reusable architecture, sequence, flow, or model diagram:

1. Generate `diagrams/{DiagramName}/index.mdx` or nest it under the relevant domain/subdomain
2. Include `id`, `name`, `version`, and `summary`
3. Put the diagram in the body as a fenced `mermaid`, `plantuml`, or other supported diagram block
4. Reference the diagram from related resources using the `diagrams` frontmatter field

### Documenting a Business Flow

When a user describes a multi-step process:

1. Identify distinct steps (user actions, service calls, message exchanges, external systems)
2. Generate the flow `index.mdx` with `steps` array
3. Each step should have `id`, `title`, and appropriate type (`actor`, `service`, `agent`, `message`, `externalSystem`)
4. Connect steps with `next_step` or `next_steps` for branching

### Documenting Channel Routing

When a user describes how messages flow through infrastructure:

1. Generate channel `index.mdx` files with `routes` for channel-to-channel routing
2. Update service or agent `sends`/`receives` with `to`/`from` fields pointing to channels
3. The full picture should show: Service or agent sends → Channel → routes to → Channel → service or agent receives

## Quality Checklist

- Take your time to do this thoroughly
- Quality is more important than speed
- Do not skip validation steps

Before delivering documentation to the user, verify every file against this checklist:

1. Frontmatter has valid YAML between `---` delimiters
2. `id` matches the folder name
3. `version` is a valid semver string
4. `summary` is concise and meaningful (not generic)
5. Message relationships (`sends`/`receives`) include `id`
6. Channel routing (`to`/`from`) references valid channel IDs
7. Body includes `<NodeGraph />` for visualization when the resource has graph relationships
8. Schema references point to real files
9. Folder structure follows catalog conventions
10. No duplicate resources (checked against existing catalog)
11. Versioned resources use `index.mdx` (or match the catalog's existing `.md`/`.mdx` convention); teams and users use `{id}.mdx`; changelogs use `changelog.mdx`/`changelog.md`
12. Every domain has at least one system, service, or agent — never create an empty domain
13. Domain `systems`, `services`, and `agents` frontmatter lists every direct system, service, and agent that belongs to that domain
14. Domain `entities`, `data-products`, `flows`, and `diagrams` frontmatter lists nested resources when present
15. Every domain has a `ubiquitous-language.mdx` file with relevant domain terms extracted from services, agents, events, commands, entities, data products, and business processes
16. ADRs have a valid status, date, decision makers when known, and `appliesTo` references for impacted resources
17. System `scope`, relationship pointers, and actor directions are valid when systems are generated
18. Data product contract files referenced in `outputs.contract.path` exist when generated

## Troubleshooting

### Messages Not Showing in Visualizer

If generated events/commands don't appear in the service or agent node graph:

- Verify the `sends`/`receives` arrays in the service or agent frontmatter reference the exact `id` of the message
- Ensure the message has its own `index.mdx` file

### Schema Not Rendering

If `<Schema />` or `<SchemaViewer />` components show errors:

- Verify `schemaPath` in frontmatter points to a file that exists alongside `index.mdx`
- Check the schema file is valid JSON/Avro/Protobuf

### Folder Structure Not Recognized

If resources don't appear in EventCatalog:

- Verify the file is named exactly `index.mdx` (not `INDEX.mdx` or `readme.md`)
- Verify the folder is inside a recognized collection directory (`systems/`, `services/`, `agents/`, `events/`, `domains/`, etc.)

### System Context Diagram Not Showing Actors or Relationships

If `<ContextDiagram />` does not show expected system context:

- Verify the system frontmatter has `relationships` or `actors`
- Verify `relationships` point to valid system IDs and include useful `label` values
- Verify actor `direction` is `inbound` or `outbound`
- If viewing a domain context diagram, verify the domain `systems` frontmatter references the relevant systems

### Channel Routing Not Visible

If channel connections don't appear in the visualizer:

- Verify the `routes` field in the channel frontmatter references valid channel IDs
- Verify the `to`/`from` fields in service or agent sends/receives reference valid channel IDs
