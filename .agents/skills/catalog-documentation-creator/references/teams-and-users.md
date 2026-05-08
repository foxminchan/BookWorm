# Teams and Users

## Teams

### Format

**File:** `{team-id}.mdx` (NOT inside a folder, NOT `index.mdx`)
**Location:** `teams/{team-id}.mdx`

### Frontmatter Fields

| Field                     | Required | Description                                          |
| ------------------------- | -------- | ---------------------------------------------------- |
| `id`                      | Yes      | Unique identifier in kebab-case (e.g., `full-stack`) |
| `name`                    | Yes      | Human-readable team name                             |
| `summary`                 | Yes      | Brief team description                               |
| `members`                 | No       | Array of user IDs                                    |
| `email`                   | No       | Team email address                                   |
| `slackDirectMessageUrl`   | No       | Slack channel URL                                    |
| `msTeamsDirectMessageUrl` | No       | MS Teams channel URL                                 |

### Example

```mdx
---
id: full-stack
name: Full stackers
summary: Full stack developers based in London, UK
members:
  - dboyne
  - rthomas
  - lkim
email: fullstack@company.com
slackDirectMessageUrl: https://yourteam.slack.com/channels/full-stack
---

## Overview

The Full Stack Team is responsible for developing and maintaining both the front-end and back-end components of the platform.

## Responsibilities

- **Front-End Development**: Design and implement user interfaces
- **Back-End Development**: Develop and maintain server-side logic
- **API Design**: Create and maintain RESTful and event-driven APIs
```

---

## Users

### Format

**File:** `{user-id}.mdx` (NOT inside a folder, NOT `index.mdx`)
**Location:** `users/{user-id}.mdx`

### Frontmatter Fields

| Field                     | Required | Description                                      |
| ------------------------- | -------- | ------------------------------------------------ |
| `id`                      | Yes      | Unique identifier in kebab-case (e.g., `dboyne`) |
| `name`                    | Yes      | Full name                                        |
| `role`                    | No       | Job title/role                                   |
| `avatarUrl`               | No       | URL to profile image                             |
| `email`                   | No       | Email address                                    |
| `slackDirectMessageUrl`   | No       | Slack DM URL                                     |
| `msTeamsDirectMessageUrl` | No       | MS Teams DM URL                                  |

### Example

```mdx
---
id: dboyne
name: David Boyne
avatarUrl: "https://pbs.twimg.com/profile_images/1262283153563140096/DYRDqKg6_400x400.png"
role: Lead developer
email: david@company.com
slackDirectMessageUrl: https://yourteam.slack.com/channels/dboyne
---

Hello! I'm David Boyne, the Tech Lead of the Full Stackers team.

### About Me

With over a decade of experience in the tech industry, I focus on building scalable and resilient system architectures.

### What I Do

- **Architecture Design**: Crafting scalable system architectures
- **Team Leadership**: Guiding a talented team of developers
- **Event-Driven Systems**: Designing event-driven microservices
```

## Key Conventions

- Teams and users use a SINGLE `.mdx` file, NOT an `index.mdx` inside a folder
- IDs should be kebab-case
- User IDs are referenced in `owners` arrays across services, events, domains, etc.
- Team IDs are also valid in `owners` arrays
- `members` in a team reference user IDs
