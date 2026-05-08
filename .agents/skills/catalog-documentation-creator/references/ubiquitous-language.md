# Ubiquitous Language

Ubiquitous language is a core concept from domain-driven design (DDD). It's a shared vocabulary used by all stakeholders — developers, product managers, architects, domain experts — to reduce misunderstandings and align communication within a bounded context.

EventCatalog supports documenting ubiquitous language terms per domain as a glossary/dictionary.

## Format

**File:** `ubiquitous-language.mdx` (NOT `index.mdx`)
**Location:** Inside a domain folder: `domains/{DomainName}/ubiquitous-language.mdx`

This file uses YAML frontmatter only — the body is typically empty.

## Frontmatter Fields

The frontmatter contains a single `dictionary` array:

```yaml
---
dictionary:
  - id: TermName
    name: Term Name
    summary: "Brief one-line description (under 125 characters)"
    description: |
      Detailed explanation of the term, its business context,
      and how it's used within this domain.
    icon: LucideIconName
---
```

### Term Fields

| Field         | Required | Description                                                                        |
| ------------- | -------- | ---------------------------------------------------------------------------------- |
| `id`          | Yes      | Unique identifier for the term                                                     |
| `name`        | Yes      | Display name                                                                       |
| `summary`     | Yes      | Brief one-line description (under 125 characters)                                  |
| `description` | No       | Detailed multi-line explanation in markdown                                        |
| `icon`        | No       | Lucide icon name (from lucide.dev) — e.g., `FileText`, `Package`, `Tag`, `Receipt` |

## When to Generate Ubiquitous Language

When documenting a domain, analyze the services, events, commands, and codebase to identify domain-specific terms that have precise meaning within that bounded context. Look for:

- **Nouns in event/command names** — `Order`, `Payment`, `Shipment`, `Subscription` are likely domain terms
- **Business concepts referenced in service descriptions** — `Fulfillment`, `Consignment`, `Invoice`
- **Entities and aggregates** — `Customer`, `Product`, `Cart`, `SKU`
- **Processes and workflows** — `Checkout`, `Return`, `Refund`, `Settlement`
- **Technical terms with domain-specific meaning** — terms that mean something specific in this context vs. general usage

CRITICAL: Always generate a `ubiquitous-language.mdx` file when creating a domain. Extract terms from the services, events, commands, and any codebase or documentation the user provides.

## Example: Orders Domain

**File:** `domains/Orders/ubiquitous-language.mdx`

```mdx
---
dictionary:
  - id: Purchase Order
    name: Purchase Order
    summary: "A document issued by a buyer to a seller indicating the types, quantities, and agreed prices for products or services."
    description: |
      A purchase order (PO) is a document that initiates the buying process between entities. It protects both buyer and seller by clearly documenting the transaction details. Key components include:

      - Unique PO number for tracking
      - Detailed item specifications and quantities
      - Agreed prices and payment terms
      - Delivery requirements and timelines
      - Terms and conditions of the purchase
    icon: FileText

  - id: Order Line
    name: Order Line
    summary: "An individual item within a purchase order, representing a specific product or service being ordered."
    description: |
      Order lines are the fundamental building blocks of any purchase order. Each line represents a distinct item or service and contains critical information for order fulfillment:

      - Product identifier (SKU or part number)
      - Quantity ordered
      - Unit price and total line value
      - Special handling instructions
      - Required delivery date

      Order lines drive warehouse picking operations, shipping processes, and financial calculations.
    icon: ListOrdered

  - id: SKU
    name: SKU
    summary: "Stock Keeping Unit - A unique identifier for distinct products and their variants in inventory."
    description: |
      SKUs are the cornerstone of effective inventory management systems. Each SKU represents a unique combination of product attributes:

      - Product variations (size, color, style)
      - Storage location identifiers
      - Supplier information
      - Reorder points and quantities

      SKUs enable precise inventory tracking, automated reordering, and detailed sales analytics.
    icon: Tag

  - id: Fulfillment
    name: Fulfillment
    summary: "The process of completing an order and delivering it to the customer."
    description: |
      Fulfillment encompasses all steps from order receipt to delivery, including:

      - Order processing and picking
      - Packaging and shipping
      - Delivery tracking and confirmation
      - Handling returns and exchanges

      Efficient fulfillment is key to customer satisfaction and operational efficiency.
    icon: PackageCheck

  - id: Consignment
    name: Consignment
    summary: "A batch of goods destined for or delivered to someone."
    description: |
      A consignment represents the physical movement of goods through the supply chain. It encompasses:

      - Packaging and labeling requirements
      - Transportation method and routing
      - Customs documentation for international shipments
      - Tracking and proof of delivery

      Consignments may combine multiple orders for efficient shipping.
    icon: Package

  - id: Invoice
    name: Invoice
    summary: "A document issued by a seller to a buyer, listing goods or services provided and the amount due."
    description: |
      Invoices are critical for financial transactions, serving as a request for payment. They include:

      - Invoice number for tracking
      - List of products or services provided
      - Total amount due and payment terms
      - Due date for payment

      Invoices are essential for accounting, tax purposes, and maintaining cash flow.
    icon: Receipt

  - id: Return
    name: Return
    summary: "The process of sending back goods to the seller for a refund or exchange."
    description: |
      Returns management is an important aspect of customer service and inventory control:

      - Processing return requests and authorizations
      - Inspecting returned items for quality
      - Restocking or disposing of returned goods
      - Issuing refunds or exchanges
    icon: PackageX

  - id: Inventory
    name: Inventory
    summary: "The complete list of items held in stock by a business."
    description: |
      Inventory management is crucial for balancing supply and demand:

      - Tracking stock levels and locations
      - Managing reorder points and quantities
      - Conducting regular stock audits
      - Analyzing inventory turnover rates
    icon: Warehouse
---
```

## Example: Payment Domain

**File:** `domains/Payment/ubiquitous-language.mdx`

```mdx
---
dictionary:
  - id: Transaction
    name: Transaction
    summary: "A completed exchange of funds between a buyer and seller for goods or services."
    description: |
      A transaction represents the financial exchange that occurs during the payment process. Key attributes:

      - Transaction ID for tracking
      - Amount and currency
      - Payment method used
      - Status (authorized, captured, failed, refunded)
      - Timestamp and audit trail
    icon: ArrowLeftRight

  - id: Settlement
    name: Settlement
    summary: "The process of transferring funds from the buyer's account to the seller's account."
    description: |
      Settlement is the final stage of payment processing where funds actually move between accounts:

      - Batch processing of transactions
      - Reconciliation with payment gateway
      - Fee deductions and net amount calculations
      - Settlement period (T+1, T+2, etc.)
    icon: Landmark

  - id: Chargeback
    name: Chargeback
    summary: "A reversal of a payment initiated by the cardholder's bank, typically due to a dispute."
    description: |
      Chargebacks occur when a customer disputes a transaction with their bank:

      - Reason codes (fraud, not received, not as described)
      - Evidence submission deadlines
      - Financial impact and fee assessment
      - Prevention strategies and fraud detection
    icon: ShieldAlert

  - id: Tokenization
    name: Tokenization
    summary: "Replacing sensitive payment data with a non-sensitive token for secure storage and processing."
    description: |
      Tokenization is a security practice that protects payment card data:

      - Original card number replaced with unique token
      - Token can be used for recurring payments
      - Reduces PCI compliance scope
      - Tokens are useless if intercepted
    icon: Lock
---
```

## Key Conventions

- One `ubiquitous-language.mdx` file per domain
- Terms automatically appear in the domain sidebar in EventCatalog
- Keep summaries under 125 characters
- Use Lucide icons (browse at lucide.dev) to make terms visually distinct
- Focus on terms that have specific meaning within the domain context — not generic programming terms
- Extract terms from event names, entity names, command names, and business processes
- Include enough description to help a new team member understand the term without prior context
