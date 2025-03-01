---
id: ordering-service_PATCH_api_v1_buyers_address
version: 1.0.0
name: Update Buyer Address
summary: Update buyer address by buyer id if exists
schemaPath: ""
badges:
  - content: PATCH
    textColor: purple
    backgroundColor: purple
  - content: "Buyer"
    textColor: yellow
    backgroundColor: yellow
owners:
  - nhanxnguyen
---

## Overview

This endpoint allows for the modification of a buyer's address information within the Ordering bounded context. In DDD terms, the Buyer is an aggregate root within the Ordering domain that contains Address as a value object.

The operation respects the invariants of the Buyer aggregate by ensuring that address changes are validated and consistently applied. Address updates are significant domain events as they can affect shipping options, tax calculations, and order fulfillment processes.

When a buyer's address is updated:

1. The system verifies the buyer exists in the Ordering context
2. Address validation rules are applied according to domain specifications
3. If valid, the address value object is replaced with the new values
4. The buyer aggregate is persisted, maintaining its consistency

This operation is particularly important for the order fulfillment process as shipping and tax calculations depend on accurate address information.

## Architecture

<NodeGraph />

## PATCH `(/api/v1/buyers/address)`

### Parameters

- **id** (path) (required): The order id

### Responses

#### <span className="text-green-500">200 OK</span>

<SchemaViewer file="response-200.json" maxHeight="500" id="response-200" />
