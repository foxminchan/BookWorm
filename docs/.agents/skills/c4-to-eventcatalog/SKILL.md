---
name: c4-to-eventcatalog
description: Converts C4 architecture models into EventCatalog documentation plans and hands off to catalog-documentation-creator for file generation. Use when user provides a C4 model, Structurizr DSL, LikeC4, C4-PlantUML, Mermaid C4, diagrams, or architecture descriptions and asks to create, update, reconcile, or map them into an EventCatalog project.
license: MIT
metadata:
  author: eventcatalog
  version: "1.0.0"
---

# C4 to EventCatalog

## Overview

Map a C4 model into EventCatalog resources without flattening architecture intent. Produce a reviewable `.c4-eventcatalog-plan.md` first, then use `catalog-documentation-creator` to generate or update the actual catalog files after the user approves the mapping.

## Load References

Always read `references/c4-mapping.md` before mapping a C4 model.

When generating files, read the relevant `../catalog-documentation-creator/references/` files:

- `domains.md`, `systems.md`, `services.md`, `agents.md`
- `events.md`, `commands.md`, `queries.md`
- `channels.md`, `containers.md`, `flows.md`
- `components.md` for NodeGraph, Mermaid, wiki links, schemas, and MDX components
- `diagrams.md` when the model has views worth keeping as standalone diagrams

## Workflow

Follow these phases in order.

### 1. Locate inputs

Ask for the C4 source if it is not provided. Accept Structurizr DSL/JSON, LikeC4 (`.c4`), C4-PlantUML, Mermaid C4, Markdown architecture docs, or image diagrams. Warn that source text is more reliable than image-only extraction.

If the source is LikeC4, EventCatalog can also render its views directly with `<LikeC4View viewId="..." />` when the `.c4` files live in the catalog and the `likec4` package is installed. Offer to embed the relevant views in the mapped resources' bodies alongside the generated resources.

Ask whether the user has an EventCatalog project. If yes, ask for the path and verify `eventcatalog.config.js` or known resource directories. If no, note that scaffolding happens during handoff through `catalog-documentation-creator`.

### 2. Inventory the existing catalog

If a catalog exists, inspect it before proposing changes:

- Detect flat vs domain-nested structure.
- Record existing resource IDs, names, versions, owners, and relationships.
- Note naming style, schema formats, owner conventions, badges, and use of domains.
- Treat existing catalog resources not present in the C4 model as `investigate`, not deletion candidates.

### 3. Parse the C4 model

Extract:

- People and external systems
- Software systems
- Containers, including apps, services, agents, databases, queues, and browsers/mobile clients
- Components only when they cross an ownership, deployment, data, or message boundary
- Relationships, protocols, technologies, tags, descriptions, and views
- Dynamic views or narrative scenarios that should become flows
- Message names, topics, queues, APIs, commands, queries, events, and schema references

Preserve evidence. For source files, cite file paths and line numbers where possible. For diagrams, cite the diagram/view name and element IDs.

### 4. Map and reconcile

Use `references/c4-mapping.md` to map each C4 element into one of `domain`, `system` (internal or `scope: external`), system `actor`, system `relationship`, `service`, `agent`, `event`, `command`, `query`, `channel`, `container`, `flow`, `document inside resource body`, or `needs user decision`.

For existing catalogs, assign each mapped item a status: `new`, `update`, `unchanged`, or `investigate`.

### 5. Ask only structural questions

Ask one question at a time. Each question must include:

- The recommended answer
- The evidence that led to it
- The consequence for generated EventCatalog resources

Only ask about decisions that affect structure: domain boundaries, which software systems are internal vs external, deployable service boundaries, agent classification, whether a relationship is an event/command/query/API call, whether an infrastructure node is a channel or data container, and whether to update an existing resource. Do not grill the user on prose, summaries, or cosmetic metadata.

### 6. Write the plan

Write `.c4-eventcatalog-plan.md` in the catalog directory when one exists, otherwise next to the supplied C4 source or in the current working directory. Include the template from `references/c4-mapping.md`: source, decisions, resource mapping, relationships, files to create/update, and open questions.

Ask the user to approve the plan before generating catalog files.

### 7. Generate catalog files

After approval, use `catalog-documentation-creator` to create or update files. Load only the relevant EventCatalog reference files for the resources in the approved plan. Follow the target catalog's existing structure and never overwrite an existing resource without preserving or reconciling user content.

## Quality Rules

- Do not create one EventCatalog resource per C4 box by default. Map based on responsibility, deployment, ownership, message flow, and EventCatalog usefulness.
- Do not create empty domains. A domain must contain at least one system, subdomain, service, or agent.
- Do not model every C4 component as an EventCatalog resource. Most components belong in the parent service body, a Mermaid diagram, or a flow.
- Prefer explicit C4 relationship labels over inferred message types. If the relationship label is ambiguous, mark it as `needs user decision`.
- Keep IDs consistent with the catalog. If creating a new catalog, prefer PascalCase resource IDs for business resources unless the user specifies otherwise.
- Preserve existing catalog content. Add, update, or flag drift; do not delete.
- Include `<NodeGraph />` in generated resources through the handoff skill wherever the relevant EventCatalog reference recommends it.
