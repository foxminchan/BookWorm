---
id: catalog-service_POST_api_v1_publishers
version: 1.0.0
name: Create Publisher
summary: Create a publisher
schemaPath: request-body.json
badges:
  - content: POST
    textColor: green
    backgroundColor: green
  - content: "Publisher"
    textColor: yellow
    backgroundColor: yellow
owners:
  - nhanxnguyen
---

## Overview

Creates a new Publisher entity in the Catalog domain. Publishers are aggregate roots that represent book publishing companies within our bounded context.

This endpoint follows Domain-Driven Design principles by implementing the Command pattern through MediatR. The command is validated using FluentValidation before being processed by the domain service.

## Architecture

<NodeGraph />

## POST `(/api/v1/publishers)`

### Parameters

- **id** (path) (required)

### Request Body

<SchemaViewer file="request-body.json" maxHeight="500" id="request-body" />

### Responses

#### <span className="text-orange-500">400 Bad Request</span>

<SchemaViewer file="response-400.json" maxHeight="500" id="response-400" />

#### <span className="text-green-500">201 Created</span>
