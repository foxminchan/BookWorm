# Supporting Collections

EventCatalog has supporting collections that are not always ordinary user-authored resources. Use this reference to avoid generating the wrong files.

## Changelogs

**File:** `changelog.mdx` or `changelog.md`
**Location:** Alongside a resource, e.g. `services/OrdersService/changelog.mdx`

Use changelogs when the user asks to document resource history or release notes.

```mdx
---
createdAt: 2026-06-18
badges:
  - content: Updated
    backgroundColor: green
    textColor: green
---

## 1.1.0

- Added `PaymentFailed` to the service receives list.
- Documented retry behavior for payment commands.
```

## Resource Docs

**Location:** Inside a resource `docs/` folder, e.g. `services/OrdersService/docs/runbooks/retry-failures.mdx`

Use resource docs for extra pages attached to a resource: ADR notes, runbooks, contracts, troubleshooting, guides, and operational procedures. Do not confuse these with first-class ADRs in `adrs/{adr-id}/index.mdx`.

```mdx
---
title: Retry Failed Orders
summary: Operational guide for retrying failed order processing.
order: 1
---

## Procedure

1. Check the order processing dashboard.
2. Confirm the failure is retriable.
3. Replay the message from the dead-letter queue.
```

Resource doc frontmatter takes `title`, `summary`, `order` (position in the sidebar), `slug` and `badges`.

Optional category files can be created under docs folders. They take only `label` and `position`:

```json
{
	"label": "Runbooks",
	"position": 1
}
```

Use `category.json` or `_category_.json` depending on the catalog's existing convention.

## Custom Docs

**Location:** `docs/**/*.mdx` (or `.md`)

Use custom docs for general documentation that is not owned by a specific EventCatalog resource. `title` and `summary` are required; `sidebar: { label, order }`, `slug`, `owners` and `badges` are optional.

```mdx
---
title: Platform Onboarding
summary: How teams publish resources into EventCatalog.
sidebar:
  label: Onboarding
  order: 1
---

## Overview

This guide explains how teams contribute catalog resources and request reviews.
```

Don't create `pages/*.mdx` for documentation. The catalog's `pages/` folder is for custom Astro pages and API routes (see the custom-pages-and-apis skill).

## Usage Examples

**Location:** An `examples/` folder inside an event, command or query, e.g. `events/OrderCreated/examples/basic.mdx`

Each `.md`/`.mdx` file (optional `title` and `summary` frontmatter) is shown in the message's Usage Examples tab. Use them for example payloads and producer/consumer snippets.

````mdx
---
title: Minimal order
summary: The smallest valid OrderCreated payload.
---

```json
{ "orderId": "ord_123", "customerId": "cus_456", "total": 42.5 }
```
````

## Custom Resource Sidebars

**File:** `sidebar.json` next to a resource's `index.mdx` (domains, systems, services, agents, messages, flows, containers, entities, data products, ADRs)

When present it fully replaces that resource's generated sidebar: only what's listed renders, in that order. Only create one when the user asks to customise a resource's sidebar.

```json
{
	"sections": [
		"$quick-reference",
		{ "section": "$owners", "title": "Team" },
		{
			"title": "Runbooks",
			"icon": "Siren",
			"pages": [
				"[[doc|runbooks/retry-failures]]",
				{ "title": "Dashboard", "href": "https://grafana.example.com/d/orders" }
			]
		}
	]
}
```

Sections are predefined `$tokens` (e.g. `$quick-reference`, `$owners`) or custom groups `{ title, icon?, collapsed?, pages }`. Pages can be `[[type|Id@version]]` resource refs, `[[doc|path]]`, `[[spec|file]]`, `[[schema|event/id@version]]`, `{ title, href }` links or nested groups.

## Schemas

Do not create generated `schemas` collection entries directly.

For events, commands, and queries:

- Put schema files next to the message `index.mdx`, or point to an external schema reference if the catalog is configured for it.
- Use `schemaPath` for a primary local schema.
- Use `schemas` when the message has multiple schemas or environment-specific schemas.

```yaml
schemaPath: schema.json
```

```yaml
schemas:
  - file: order-created.json
    name: Order Created JSON Schema
    format: jsonschema
    default: true
  - ref: git://contracts/events/OrderCreated.avsc
    name: Order Created Avro
    format: avro
```

EventCatalog derives the `schemas` collection from these message references.

## Studio Designs

The `designs` collection is loaded from `.ecstudio` files produced by EventCatalog Studio. Do not invent `.ecstudio` files unless the user explicitly asks for Studio design content and provides enough structure to generate valid JSON.

## Internal Collections

`customPages`, `resourceDocs` and `resourceDocCategories` are the collection names behind custom docs, resource docs and their category files. They're available in every catalog (no plan or license needed); match the catalog's existing structure when generating those files.
