# Data Products

## Format

**File:** `index.mdx` inside a data product folder
**Location:** `data-products/{DataProductName}/index.mdx` or nested under a domain/subdomain, e.g. `domains/{Domain}/data-products/{DataProductName}/index.mdx`

Data products document curated data outputs such as analytics marts, reporting datasets, feature stores, ML inputs, operational read models, or externally consumed data contracts.

## Frontmatter Fields

| Field          | Required | Description                                                                                         |
| -------------- | -------- | --------------------------------------------------------------------------------------------------- |
| `id`           | Yes      | Unique identifier, often kebab-case (e.g., `order-analytics`)                                       |
| `name`         | Yes      | Human-readable name                                                                                 |
| `version`      | Yes      | Semver string (e.g., `1.0.0`)                                                                       |
| `summary`      | Yes      | 1-2 sentence description of the data product                                                        |
| `owners`       | Yes      | Array of team or user IDs                                                                           |
| `inputs`       | No       | Upstream resources consumed by the data product                                                     |
| `outputs`      | No       | Downstream resources, messages, containers, or contracts produced by the data product               |
| `badges`       | No       | Array of badge objects                                                                              |
| `repository`   | No       | Object with `language` and `url`                                                                    |
| `detailsPanel` | No       | Visibility toggles for domains, inputs, outputs, versions, repository, owners, changelog, and flows |

## Inputs and Outputs

`inputs` are pointers with `id` and optional `version`.

`outputs` are pointers with `id`, optional `version`, and optional `contract`:

```yaml
outputs:
  - id: orders-db
    contract:
      path: fact-orders-contract.json
      name: Fact Orders Contract
      type: json-schema
```

Contract files should sit next to the data product `index.mdx` unless the catalog already uses a different convention.

## Example

````mdx
---
id: order-analytics
name: Order Analytics
version: 1.0.0
summary: Aggregated order metrics and KPIs for business intelligence, reporting dashboards, and downstream analytics systems.
styles:
  icon: /icons/analytics/databricks.svg
owners:
  - data-platform
badges:
  - content: Analytics
    backgroundColor: neutral
    textColor: neutral
  - content: dbt
    backgroundColor: neutral
    textColor: neutral
inputs:
  - id: OrderConfirmed
  - id: PaymentProcessed
  - id: payment-cache
outputs:
  - id: OrderMetricsCalculated
  - id: orders-db
    contract:
      path: fact-orders-contract.json
      name: Fact Orders Contract
      type: json-schema
---

The Order Analytics data product transforms raw order and payment events into aggregated metrics optimized for reporting and business intelligence.

<NodeGraph />

<SchemaViewer file="fact-orders-contract.json" />

## Overview

This data product consumes order lifecycle events and produces denormalized, query-optimized tables for analytics workloads.

## Output Tables

### `fact_orders`

The primary fact table containing one row per order with pre-computed metrics.

```sql
CREATE TABLE fact_orders (
  order_id UUID PRIMARY KEY,
  customer_id UUID NOT NULL,
  order_date DATE NOT NULL,
  status VARCHAR(20),
  gross_amount DECIMAL(12,2),
  net_amount DECIMAL(12,2),
  currency CHAR(3)
);
```

## SLAs

| Metric         | Target       |
| -------------- | ------------ |
| Data Freshness | < 15 minutes |
| Availability   | 99.9%        |

## Access Patterns

- BI tools connect through the analytics warehouse.
- dbt downstream models reference the `fact_orders` model.
- Airflow runs aggregation jobs on a schedule.
````

## Key Conventions

- Use data products when documenting data outputs with clear consumers, ownership, contracts, or SLAs.
- Use `inputs` and `outputs` to build lineage in the visualizer.
- Include `<NodeGraph />` to show upstream and downstream relationships.
- Include `<SchemaViewer />` when output contracts are available.
- Link the data product from the owning domain with:
  ```yaml
  data-products:
    - id: order-analytics
  ```
