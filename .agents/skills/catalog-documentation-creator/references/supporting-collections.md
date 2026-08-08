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
---

## Procedure

1. Check the order processing dashboard.
2. Confirm the failure is retriable.
3. Replay the message from the dead-letter queue.
```

Optional category files can be created under docs folders:

```json
{
  "label": "Runbooks",
  "collapsed": false
}
```

Use `category.json` or `_category_.json` depending on the catalog's existing convention.

## Custom Docs and Pages

**Location:** `docs/**/*.mdx` for custom documentation, and `pages/*.mdx` for pages.

Use custom docs for general documentation that is not owned by a specific EventCatalog resource.

```mdx
---
title: Platform Onboarding
summary: How teams publish resources into EventCatalog.
---

## Overview

This guide explains how teams contribute catalog resources and request reviews.
```

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

## Internal/Enterprise Collections

`customPages`, `resourceDocs`, and `resourceDocCategories` may depend on enterprise/custom documentation features. Match the catalog's existing structure and configuration before generating those files.
