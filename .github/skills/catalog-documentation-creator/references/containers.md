# Containers

Containers represent infrastructure components that services interact with — databases, caches, message queues, object stores, read models, etc.

## Format

**File:** `index.mdx` inside a container folder
**Location:** Nested under a service: `services/{ServiceName}/containers/{ContainerName}/index.mdx`

## Frontmatter Fields

| Field            | Required | Description                                                                 |
| ---------------- | -------- | --------------------------------------------------------------------------- |
| `id`             | Yes      | Unique identifier in kebab-case (e.g., `inventory-db`)                      |
| `name`           | Yes      | Human-readable name                                                         |
| `version`        | Yes      | Semver string (e.g., `0.0.1`)                                               |
| `summary`        | Yes      | Brief description of what this container stores/does                        |
| `container_type` | No       | Type of container (e.g., `database`, `cache`, `queue`, `storage`)           |
| `technology`     | No       | Technology and version (e.g., `postgres@14`, `redis@7`, `elasticsearch@8`)  |
| `authoritative`  | No       | `true` if this is the system of record, `false` for read models/projections |
| `access_mode`    | No       | `read`, `readWrite`, or `write`                                             |
| `classification` | No       | MUST be one of: `public`, `internal`, `confidential`, `regulated`           |
| `retention`      | No       | Data retention period (e.g., `5y`, `30d`, `indefinite`)                     |
| `residency`      | No       | Data residency/region (e.g., `eu-west-1`)                                   |

## How Services Connect to Containers

Services use `writesTo` and `readsFrom` fields in their frontmatter:

```yaml
# In the service's index.mdx
writesTo:
  - id: inventory-db
    version: 0.0.1
  - id: inventory-readmodel
    version: 0.0.1
readsFrom:
  - id: inventory-db
    version: 0.0.1
  - id: inventory-readmodel
    version: 0.0.1
```

## Example 1: Authoritative Database

A primary database that is the system of record.

````mdx
---
id: inventory-db
name: Inventory DB
version: 0.0.1
container_type: database
technology: postgres@14
authoritative: true
access_mode: readWrite
classification: internal
retention: 5y
residency: eu-west-1
summary: Authoritative database for product inventory levels, warehouse stock, and inventory movements
---

<NodeGraph />

## What is this?

Inventory DB is the system of record for real-time inventory tracking across multiple warehouses and fulfillment centers. It maintains accurate stock levels, handles inventory reservations, and tracks all inventory movements.

### What does it store?

- **Inventory Levels**: Current stock quantities by product and warehouse
- **Inventory Reservations**: Temporary holds on inventory for pending orders
- **Stock Movements**: Audit trail of all inventory transactions (in, out, adjustments)
- **Reorder Points**: Low-stock thresholds and automatic reorder triggers

### Who writes to it?

- **InventoryService** manages stock levels, reservations, and adjustments
- **OrdersService** creates reservations when orders are placed

### Who reads from it?

- **OrdersService** checks stock availability before order confirmation
- **InventoryService** monitors low-stock alerts and reorder points
- **Analytics** tracks inventory turnover and stock-out rates

### Common queries

```sql
-- Check available inventory for a product
SELECT
  il.warehouse_id,
  il.quantity_on_hand,
  COALESCE(SUM(ir.quantity), 0) AS quantity_reserved,
  il.quantity_on_hand - COALESCE(SUM(ir.quantity), 0) AS quantity_available
FROM inventory_levels il
LEFT JOIN inventory_reservations ir
  ON ir.product_id = il.product_id
  AND ir.warehouse_id = il.warehouse_id
  AND ir.expires_at > NOW()
WHERE il.product_id = $1
GROUP BY il.warehouse_id, il.quantity_on_hand;
```
````

### Access patterns and guidance

- Use indexed lookups by `product_id` and `warehouse_id`
- Reservations have TTL; expired reservations cleaned up hourly
- Stock movements are append-only for audit compliance
- Use pessimistic locking for concurrent inventory updates

### Requesting access

1. **Read-only access**: Submit request via ServiceNow, approval from inventory team lead
2. **Write access**: Restricted to InventoryService and authorized systems only

**Contact**: Slack #inventory-team

````

## Example 2: Read Model / Projection

A non-authoritative read-optimized projection.

```mdx
---
id: inventory-readmodel
name: Inventory Read Model
version: 0.0.1
container_type: database
technology: postgres@14
authoritative: false
access_mode: read
summary: Projection of stock levels from Inventory domain
---

<NodeGraph />

The Inventory Read Model is a PostgreSQL database that serves as an optimized projection of inventory data for high-performance read operations.

## Overview

This read model is maintained by the Inventory Service and provides denormalized views of inventory data optimized for query performance. It serves as the primary data source for:

- Real-time stock level checks during order processing
- Inventory reporting and analytics
- Product availability displays on the storefront

## Performance Characteristics

- **Read Latency**: < 5ms for single product queries
- **Throughput**: 50,000+ queries per second
- **Data Freshness**: Near real-time (< 100ms from event occurrence)

## Data Sources

This read model is populated from the following event streams:

- **InventoryAdjusted** - Manual stock adjustments from warehouse operations
- **InventoryReceived** - Updates available quantity when new stock arrives
- **InventoryReserved** - Increases reserved quantity for orders
````

## Example 3: Regulated Database

A database with compliance requirements.

```mdx
---
id: payments-db
name: Payments DB
version: 0.0.1
container_type: database
technology: postgres@15
authoritative: true
access_mode: readWrite
classification: regulated
retention: 10y
residency: eu-west-1
summary: Primary database for payment transactions and payment method records
---

<NodeGraph />

### What is this?

Payments DB is the authoritative database for all payment transactions, payment methods, and related financial data.

### What does it store?

- **Payments**: Transaction records including amount, currency, status, timestamps
- **Payment Methods**: Tokenized payment method details (PCI-compliant)
- **Refunds**: Refund records linked to original payment transactions

### Security and compliance

- **PCI DSS Level 1 compliance**: Card data tokenized, no raw card numbers stored
- **Encryption at rest**: Database encrypted using AWS KMS
- **Access control**: Role-based access, audit logging enabled

### Retention and residency

- **Retention**: 10 years (regulatory requirement for financial records)
- **Residency**: `eu-west-1` (GDPR compliance, data localization)
```

## Key Conventions

- Containers are always nested under the service that owns them
- Use `<NodeGraph />` to show which services read from and write to this container
- Document "Who writes to it?" and "Who reads from it?" for clear ownership
- Include common SQL queries when applicable
- Document access request procedures
- For read models, explain the data source (events that populate it)
- `authoritative: true` means this is the system of record; `false` means it's a projection/cache
