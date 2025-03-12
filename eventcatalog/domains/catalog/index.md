---
id: catalog
name: Catalog
version: 1.0.0
services:
  - id: Product Service
    version: 1.0.0
badges:
  - content: Core Domain
    backgroundColor: green
    textColor: green
  - content: Domain Model
    backgroundColor: orange
    textColor: orange
owners:
  - nhanxnguyen
---

## Overview

The Catalog domain is responsible for managing the products that are available for purchase in the BookWorm platform. It serves as the central repository for all book-related information including metadata, inventory status, pricing, and categorization.

This domain allows customers to browse the extensive collection of books, view detailed product information including author details, publisher information,... and preview content when available. The search functionality enables users to find books based on various criteria such as title, author, genre, publication date, and keywords.

The domain also provides robust management capabilities for product categories, series, collections, and publishers. It maintains relationships between books, such as series orders, related titles, and author bibliographies.

For administrators, the Catalog domain offers inventory management tools, allowing them to add new books, update existing information, manage stock levels, and control pricing and discounts.

As a core domain in the BookWorm ecosystem, Catalog integrates with other services like the Basket domain for purchasing flows, the Rating domain for customer reviews, and the Ordering domain for inventory verification during checkout processes.

<Tiles >
    <Tile icon="UserGroupIcon" href="/docs/users/nhanxnguyen" title="Contact the author" description="Any questions? Feel free to contact the owners" />
    <Tile icon="RectangleGroupIcon" href={`/visualiser/domains/${frontmatter.id}/${frontmatter.version}`} title={`${frontmatter.services.length} services are in this domain`} description="This service sends messages to downstream consumers" />
</Tiles>

## Bounded context

<NodeGraph />

<MessageTable format="all" limit={4} />
