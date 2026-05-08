---
name: catalog-documentation-creator
description: Generates EventCatalog documentation files (services, events, commands, queries, domains, flows, channels, containers) with correct frontmatter, folder structure, and best practices. Use when user asks to "document a service", "create EventCatalog files", "add an event to the catalog", "document my architecture", "generate catalog documentation", "create documentation for my microservice", or "document a database".
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
- Verify it looks like an EventCatalog project by checking for an `eventcatalog.config.js` file or known directories (`services/`, `events/`, `domains/`, etc.)
- Read the existing structure to understand whether they use **nested** (domains/services/events) or **flat** (top-level services/, events/) organization

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

- A single service and its messages
- An event, command, or query
- A full domain with nested services
- A business flow across services
- A channel (Kafka topic, RabbitMQ queue, etc.)
- A container (database, cache, queue)

Gather this information before generating:

- Resource name and purpose
- Version (default to `0.0.1` for new resources)
- Message relationships (what it sends/receives)
- Channel routing (what channels messages flow through)
- Containers (what databases/caches the service reads from or writes to)
- Schema format if applicable (JSON Schema, Avro, Protobuf)

If the user points you at a codebase (not the catalog), analyze it to extract services, messages, schemas, and relationships — then generate the corresponding catalog documentation.

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
- `references/events.md` — Events with schemas, payload examples, producer/consumer code
- `references/commands.md` — Commands with REST operations and schemas
- `references/queries.md` — Queries with REST operations and response schemas
- `references/domains.md` — Domains with subdomains, services, and business context
- `references/flows.md` — Business flows with steps, branching, and external systems
- `references/channels.md` — Channels with routing, protocols, and parameters
- `references/containers.md` — Containers (databases, caches, queues) with data classification
- `references/ubiquitous-language.md` — Ubiquitous language terms per domain (DDD glossary/dictionary)
- `references/teams-and-users.md` — Teams and users (ownership)
- `references/components.md` — Components (NodeGraph, Schema, Mermaid, Tabs, etc.) and resource references (`[[type|Name]]` wiki-style links)

Every resource file MUST include:

- Valid YAML frontmatter between `---` delimiters
- `id` field matching existing catalog conventions
- `name` as human-readable display name
- `version` as semantic version string
- `summary` as a concise 1-2 sentence description

CRITICAL: Always use `index.mdx` as the filename for resources (services, events, commands, queries, domains, flows, channels). Teams and users use `{id}.mdx` files directly. Place files in the correct folder path following the nested structure pattern:

```
domains/{DomainName}/services/{ServiceName}/events/{EventName}/index.mdx
```

Or flat structure if the catalog uses that pattern:

```
services/{ServiceName}/index.mdx
events/{EventName}/index.mdx
```

### Step 5: Validate the Output

Before presenting the files to the user, verify:

- YAML frontmatter has `---` delimiters on both sides
- All `id` fields are consistent (no spaces, match folder name)
- All `version` fields are valid semver strings (e.g., `0.0.1`)
- All message references in `sends`/`receives` include `id` and optionally `version`
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
5. Add example payload sections for each message
6. Place files in the correct nested folder structure

### Documenting a Domain

CRITICAL: A domain MUST have at least one service. Never create a domain without services in it. If the user describes a domain, ensure services are identified and generated for it.

When a user wants to document a full domain:

1. Identify the services that belong to this domain. If the user hasn't specified services, ask them: "What services belong to this domain?" Do NOT create a domain without services.
2. Generate the domain `index.mdx` with the `services` field listing every service
3. Generate each service within the domain
4. Generate each message referenced by the services
5. Generate channels if the user describes messaging infrastructure
6. Use the nested folder structure: `domains/{Domain}/services/{Service}/events/{Event}/`
7. Generate a `ubiquitous-language.mdx` file for the domain by extracting domain-specific terms from service names, event/command names, entities, and business processes. Place it at `domains/{Domain}/ubiquitous-language.mdx`. See `references/ubiquitous-language.md` for format and examples.
8. CRITICAL: After generating all files, verify the domain's frontmatter `services` field lists every service that belongs to it. Every service created under a domain MUST be referenced in the domain's `index.mdx`:
   ```yaml
   services:
     - id: OrdersService
     - id: InventoryService
     - id: PaymentService
   ```
   If a service is nested inside the domain folder but not listed in the domain's `services` frontmatter, it will not appear as part of that domain. Always cross-check.

### Documenting a Business Flow

When a user describes a multi-step process:

1. Identify distinct steps (user actions, service calls, message exchanges, external systems)
2. Generate the flow `index.mdx` with `steps` array
3. Each step should have `id`, `title`, and appropriate type (`actor`, `service`, `message`, `externalSystem`)
4. Connect steps with `next_step` or `next_steps` for branching

### Documenting Channel Routing

When a user describes how messages flow through infrastructure:

1. Generate channel `index.mdx` files with `routes` for channel-to-channel routing
2. Update service `sends`/`receives` with `to`/`from` fields pointing to channels
3. The full picture should show: Service A sends → Channel → routes to → Channel → Service B receives

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
7. Body includes `<NodeGraph />` for visualization
8. Schema references point to real files
9. Folder structure follows catalog conventions
10. No duplicate resources (checked against existing catalog)
11. File is named `index.mdx` (not `index.md`, `README.md`, or anything else)
12. Every domain has at least one service — never create an empty domain
13. Domain `services` frontmatter lists every service that belongs to that domain
14. Every domain has a `ubiquitous-language.mdx` file with relevant domain terms extracted from services, events, commands, and business processes

## Troubleshooting

### Messages Not Showing in Visualizer

If generated events/commands don't appear in the service's node graph:

- Verify the `sends`/`receives` arrays in the service frontmatter reference the exact `id` of the message
- Ensure the message has its own `index.mdx` file

### Schema Not Rendering

If `<Schema />` or `<SchemaViewer />` components show errors:

- Verify `schemaPath` in frontmatter points to a file that exists alongside `index.mdx`
- Check the schema file is valid JSON/Avro/Protobuf

### Folder Structure Not Recognized

If resources don't appear in EventCatalog:

- Verify the file is named exactly `index.mdx` (not `INDEX.mdx` or `readme.md`)
- Verify the folder is inside a recognized collection directory (`services/`, `events/`, `domains/`, etc.)

### Channel Routing Not Visible

If channel connections don't appear in the visualizer:

- Verify the `routes` field in the channel frontmatter references valid channel IDs
- Verify the `to`/`from` fields in service sends/receives reference valid channel IDs
