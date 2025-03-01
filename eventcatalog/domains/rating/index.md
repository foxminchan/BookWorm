---
id: rating
name: Rating
version: 1.0.0
services:
  - id: Rating Service
    version: 1.0.0
badges:
  - content: Core Domain
    backgroundColor: green
    textColor: green
  - content: Active Record
    backgroundColor: green
    textColor: green
owners:
  - nhanxnguyen
---

## Overview

The Rating domain is responsible for collecting and managing customer ratings and reviews for products and services. It provides a platform for customers to share their feedback and experiences with other users, helping them make informed decisions when purchasing items.

<Tiles >
    <Tile icon="UserGroupIcon" href="/docs/users/nhanxnguyen" title="Contact the author" description="Any questions? Feel free to contact the owners" />
    <Tile icon="RectangleGroupIcon" href={`/visualiser/domains/${frontmatter.id}/${frontmatter.version}`} title={`${frontmatter.services.length} services are in this domain`} description="This service sends messages to downstream consumers" />
</Tiles>

### Key Responsibilities

- Collecting and storing customer ratings and reviews for products and services
- Displaying ratings and reviews on product detail pages
- Calculating average ratings and review counts for products
- Supporting moderation and flagging of inappropriate content
- Enabling customers to rate and review products and services
- Providing APIs for retrieving ratings and reviews

## Architecture diagram

<NodeGraph />

<MessageTable format="all" limit={4} />
