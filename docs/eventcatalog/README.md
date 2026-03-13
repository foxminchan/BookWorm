[![Netlify Status](https://api.netlify.com/api/v1/badges/ff82b0cb-bbb5-4d49-b326-e4792d673420/deploy-status)](https://app.netlify.com/projects/bookwormdev/deploys)

# BookWorm EventCatalog

A living, event-driven architecture catalog for [BookWorm](https://github.com/foxminchan/BookWorm)
that documents all domain events, services, channels, and message contracts flowing through the
microservices platform. Built with [EventCatalog](https://www.eventcatalog.dev/).

The catalog is published to Netlify at **<https://bookwormdev.netlify.app/>** and rebuilds
automatically on every push to `main`.

## Tech Stack

| Tool                                                                        | Version | Purpose                         |
| --------------------------------------------------------------------------- | ------- | ------------------------------- |
| [EventCatalog](https://www.eventcatalog.dev/)                               | latest  | Catalog generator               |
| [EventCatalog Linter](https://www.eventcatalog.dev/docs/development/linter) | ^1.0    | Schema validation               |
| [Prettier](https://prettier.io/)                                            | ^3.8    | Markdown formatting             |
| [Bun](https://bun.sh/)                                                      | latest  | Package manager & script runner |

## Prerequisites

- [Node.js](https://nodejs.org/en/download/) >= 25.0.0
- [Bun](https://bun.sh/) >= 1.0 (used as the package manager and script runner)

## Getting Started

```bash
# Install dependencies
bun install

# Start the local development server (hot-reload enabled)
bun run dev
```

The dev server opens at <http://localhost:3001> by default.

## Available Scripts

| Script             | Description                                         |
| ------------------ | --------------------------------------------------- |
| `bun run dev`      | Start the development server with live reload       |
| `bun run build`    | Build the production static site                    |
| `bun run start`    | Serve the built production site locally             |
| `bun run preview`  | Preview the production build                        |
| `bun run generate` | Run generators to produce catalog artifacts         |
| `bun run lint`     | Validate catalog files with the EventCatalog linter |
| `bun run format`   | Format all files with Prettier                      |
| `bun run check`    | Run both linter and Prettier check in one step      |

## Catalog Structure

```
docs/eventcatalog/
├── domains/
│   └── Store/                     # Store bounded context
│       ├── subdomains/
│       │   ├── Catalog/           # Book catalog & inventory events
│       │   ├── Orders/            # Order processing events
│       │   └── Integration/       # Cross-service integration events
│       └── ubiquitous-language.mdx
├── channels/                      # Kafka topics and messaging channels
├── pages/                         # Custom catalog pages
├── teams/                         # Team ownership definitions
├── users/                         # Contributor profiles
├── public/                        # Static assets (logo, images)
├── eventcatalog.config.js         # Catalog configuration
└── eventcatalog.styles.css        # Theme overrides
```

## Content Guide

### Adding a New Event

1. Create a folder under the appropriate subdomain, e.g.
   `domains/Store/subdomains/Catalog/events/BookCreated/`.
2. Add an `index.mdx` file with the required front matter:

   ```yaml
   ---
   id: BookCreated
   name: BookCreated
   version: 1.0.0
   summary: Emitted when a new book is added to the catalog
   producers:
     - id: catalog-service
       version: latest
   consumers:
     - id: notification-service
       version: latest
   ---
   ```

3. Document the event payload with a JSON Schema or inline schema block.

### Adding a New Service

1. Create a folder under the relevant subdomain, e.g.
   `domains/Store/subdomains/Catalog/services/catalog-service/`.
2. Add an `index.mdx` with `id`, `name`, `version`, `summary`, `sends`, and `receives` fields.

### Adding a Channel

1. Create a folder under `channels/`, e.g. `channels/catalog-events/`.
2. Add an `index.mdx` describing the Kafka topic name, protocol, and address.

### Linting

Run `bun run lint` before opening a pull request to catch schema validation errors early.

## Deployment

The catalog is deployed automatically to [Netlify](https://bookwormdev.netlify.app/) when
changes inside `docs/eventcatalog/` are pushed to `main`.

To build and preview locally:

```bash
bun run build
bun run preview
```

## Related Resources

- 📖 [Live Catalog](https://bookwormdev.netlify.app/)
- 📚 [Architecture Documentation](https://foxminchan.github.io/BookWorm)
- 🐙 [GitHub Repository](https://github.com/foxminchan/BookWorm)
- 📘 [EventCatalog Docs](https://www.eventcatalog.dev/docs)
