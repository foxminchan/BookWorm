# BookWorm architecture catalog

This repository is the living architecture documentation for BookWorm, an online bookstore and commerce platform. It uses EventCatalog to describe the business domains, systems, services, flows, contracts, and operational guardrails that shape the product.

The catalog is intended to be read by product, engineering, platform, and operations teams. It provides a shared view of the business model, the ownership boundaries, and the integration contracts that connect domain behaviors.

## What this repository documents

BookWorm is organized around several bounded contexts:

- Catalog: the source of truth for books, pricing, availability, and discovery metadata
- Shopping: basket and checkout behavior before a purchase is accepted
- Ordering: lifecycle and status of accepted orders
- Reviews: customer feedback and rating projections
- Conversational: AI-assisted customer experiences
- Platform: shared infrastructure, notifications, and scheduled work

The domain model is expressed in the catalog under `domains/BookWorm`, while the concrete technical architecture and message contracts live in the generated catalog resources under `systems/`, `services/`, `channels/`, `flows/`, `teams/`, and `users/`.

## Quick start

```sh
bun install
bun run dev
```

Then open http://localhost:3000.

Useful commands:

```sh
bun run build
bun run fitness
bun run lint
bun run check
bun run editor
```

## Documentation map

| Area                               | Purpose                                                |
| ---------------------------------- | ------------------------------------------------------ |
| `README.md`                        | Repository-level onboarding and quick reference        |
| `domains/BookWorm/`                | Strategic domain overview and business context         |
| `domains/BookWorm/docs/`           | Getting-started and operational guidance               |
| `domains/BookWorm/subdomains/`     | Domain-by-domain architecture details                  |
| `systems/`                         | Project-level system documentation                     |
| `flows/`                           | End-to-end journeys and process diagrams               |
| `channels/`                        | Event and message channel definitions                  |
| `teams/` and `users/`              | Ownership and actor metadata                           |
| `fitness/`                         | Architecture validation rules and checks               |
| `public/`, `components/`, `pages/` | Catalog UI customization and local presentation layers |

## Reading guide

### If you are new to the system

Start with:

1. `domains/BookWorm/docs/01-getting-started.mdx`
2. `domains/BookWorm/index.mdx`
3. The relevant subdomain page such as `domains/BookWorm/subdomains/Catalog/index.mdx`

### If you are changing a system or service

Read the owning domain and the specific system page first, then inspect:

- the service contract pages
- the relevant commands/events/queries
- the ADRs for architectural decisions
- the runbooks for operational failure handling

### If you are assessing ownership or boundaries

Check the domain, team, and service metadata together with the documented invariants. BookWorm intentionally keeps context ownership explicit so changes do not silently cross business boundaries.

## How this catalog is maintained

This repository follows a docs-as-code workflow:

1. Update the relevant MDX or metadata file.
2. Run the local catalog and review the rendered output.
3. Validate the catalog with `bun run check` or `bun run fitness`.
4. Commit the documentation alongside any matching implementation change.

When a change affects a business capability or contract, update the owning docs in the same review. The catalog is a source of truth for the architecture narrative, not a purely static reference.

## Typical BookWorm flow

1. A user searches or browses the catalog.
2. Catalog provides a consistent product proposition with pricing and availability.
3. Shopping adds items to a basket and validates purchase intent.
4. Ordering accepts the order and owns the lifecycle after acceptance.
5. Reviews captures feedback and rating signals.
6. Platform handles notifications, scheduled jobs, and cross-cutting infrastructure.

## Contribution guidance

Keep documentation concrete and decision-oriented. Prefer:

- business purpose and ownership
- explicit boundaries and invariants
- contract and message names that match the implemented language
- operational notes and runbooks for known risks

Avoid vague placeholders, duplicated descriptions, or undocumented assumptions across domains.

## Architecture fitness and quality checks

The project includes EventCatalog fitness rules to validate architecture quality. These checks help ensure that services have owners, message contracts are documented, boundaries remain explicit, and operationally sensitive data stores are not silently shared across too many writers.

Run:

```sh
bun run fitness
```

The latest results are written to `.eventcatalog/fitness/results.json`.

## License

This catalog is built with EventCatalog and is designed to document BookWorm's architecture and business decisions. The repository's code and documentation live under the project license in the root of the source repository.
