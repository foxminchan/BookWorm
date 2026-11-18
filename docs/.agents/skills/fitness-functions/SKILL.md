---
name: fitness-functions
description: Writes, runs and debugs EventCatalog architecture fitness functions — TypeScript rules in `*.fitness.ts` files that check the documented architecture (ownership, naming, domain boundaries, dependencies, data access, schemas) and fail CI when it drifts. Turns an architecture rule described in plain language into a working fitness function, places it globally or beside the domain, system, service, message or data store it governs, runs `eventcatalog fitness check`, verifies the result, and wires it into CI. Use when user asks to "write a fitness function", "add an architecture rule", "enforce that every event has an owner", "check domain boundaries", "stop services sharing databases", "detect circular dependencies", "fail the build when the architecture breaks a rule", "run fitness checks in CI", or "why is my fitness function failing".
license: MIT
metadata:
  author: eventcatalog
  version: "1.0.0"
---

# EventCatalog Fitness Functions

Fitness functions are architecture rules written as code. Each one is a TypeScript file that reads the catalog through a read-only SDK and returns the **violations** it finds. `eventcatalog fitness check` runs them all, prints the results, writes a JSON report and exits non-zero when a rule fails, so the rules can gate pull requests.

The team decides what a healthy architecture looks like; EventCatalog runs the rules and reports the results.

> Fitness functions check the architecture the catalog **documents**, not the running systems. A rule is only as good as the catalog data it reads.

## Requirements (check these FIRST)

1. **Version** — `@eventcatalog/core` **5.3.0 or later** (check the catalog's `package.json`). Alternatively `@eventcatalog/cli` **1.4.0 or later** runs the same check without core (`npx @eventcatalog/cli fitness check`). Node.js 20 or later.
2. **Editor types (optional)** — fitness functions run without installing anything else, but for types and autocomplete install `@eventcatalog/fitness` as a dev dependency. Catalogs created with `create-eventcatalog` 4.5.0 or later already have it, plus two starter functions in `fitness/` and an `npm run fitness` script.

## Fitness function, linter or governance?

Pick the right tool before writing code:

| The rule is about…                                                                                                         | Use                                                          |
| -------------------------------------------------------------------------------------------------------------------------- | ------------------------------------------------------------ |
| A single file being valid: frontmatter fields, required properties, broken references, file layout                         | `@eventcatalog/linter`                                       |
| What **changed** in a pull request (a new consumer, a schema change, a deprecation), with notifications                    | `npx @eventcatalog/cli governance check` (`governance.yaml`) |
| The **shape of the architecture**: ownership, naming, boundaries, dependencies, who can access what, across many resources | **A fitness function** (this skill)                          |

## Instructions

### Step 1: Locate the catalog and what already exists

Find the catalog (the folder with `eventcatalog.config.js`) and check:

- Is there a root `fitness/` folder, or `fitness/` folders inside resource folders? Read the existing functions — reuse their helpers and match their style, and don't create a rule that duplicates one.
- Fitness function **ids must be unique** across the catalog. List the ids already in use.
- Does `package.json` have a `fitness` script? Is `@eventcatalog/fitness` installed?
- Is `.eventcatalog/fitness/` in `.gitignore`? The check writes its report there; it shouldn't be committed.

### Step 2: Turn the rule into a precise check

A rule like "services should own their data" is too vague to code. Pin down, asking the user when it isn't clear:

- **What exactly is a violation?** ("A service writes to a data store that belongs to another domain's systems.")
- **Which resources does it apply to?** All of them, one domain, one system, one service?
- **Latest versions only, or every version?** Almost always latest (`{ latestOnly: true }`).
- **Severity** — `error` fails the check (and CI); `warning` is reported but doesn't fail it. Start new rules on an existing catalog as `warning` if they'll find many violations, and tighten later.
- **What should each violation point at, and how is it fixed?** Every violation should name the resources involved and suggest the fix.

Check the catalog data supports the rule: if the rule needs `writesTo`, `owners` or `sends.to` and the catalog doesn't document them, say so — the rule would pass vacuously.

### Step 3: Choose where the file goes

Where the file lives decides the `resource` it receives — not what it can see: every function can query the whole catalog.

| Rule applies to             | Put it in                                                 | `resource` is                        |
| --------------------------- | --------------------------------------------------------- | ------------------------------------ |
| The whole catalog           | `fitness/<name>.fitness.ts` (subfolders allowed)          | `undefined`                          |
| One domain                  | `domains/Orders/fitness/<name>.fitness.ts`                | the Orders domain                    |
| One system                  | `…/systems/checkout-system/fitness/<name>.fitness.ts`     | the system                           |
| One service                 | `…/services/OrderService/fitness/<name>.fitness.ts`       | the service                          |
| One event, command or query | `…/events/OrderCreated/fitness/<name>.fitness.ts`         | the message                          |
| One data store              | `…/containers/payment-database/fitness/<name>.fitness.ts` | the data store (`type: 'container'`) |
| One specific version        | `…/versioned/1.0.0/fitness/<name>.fitness.ts`             | that version                         |

Channels, flows, entities, data products, ADRs and diagrams work the same way. Rules:

- Only files ending in **`.fitness.ts`** run. Other `.ts` files (shared helpers) are never run, so fitness functions can import them.
- The `fitness/` folder must sit **directly inside** the resource's folder (beside its `index.mdx`). A `.fitness.ts` file outside any `fitness/` folder is not run and is reported as a diagnostic. A `fitness/` folder that isn't directly inside a resource folder (or whose `index.mdx` has no `id`) makes the function an **error**, which fails the check.
- Fitness functions in federated catalogs (`federated/…`) are not run, and federation does not copy `fitness/` folders.

### Step 4: Write the fitness function

```ts
// domains/Orders/fitness/no-shared-data-stores.fitness.ts
import { defineFitnessFunction } from "@eventcatalog/fitness";

export default defineFitnessFunction({
	id: "orders-no-shared-data-stores", // unique across the catalog
	name: "Orders services only use Orders data stores",
	description: "Reaching into another domain’s database couples the domains together.",
	severity: "error", // or 'warning'; defaults to 'error'

	async run({ resource, catalog }) {
		if (resource?.type !== "domain") throw new Error("Expected to run beside a domain");

		// ...read what you need from `catalog`, decide what's wrong...

		return [
			{
				message: "Order Service writes to payment-database, which is outside the Orders domain",
				resources: [
					{ type: "service", id: "order-service", version: "1.0.0" },
					{ type: "container", id: "payment-database" },
				],
				details: "Optional extra context",
				suggestion: "Get payment data through the Payments API or its events.",
			},
		]; // [] means the rule passed
	},
});
```

Conventions when writing them:

- **`export default defineFitnessFunction({ ... })`** — one function per file. `run` can be sync or async.
- **Return violations, don't throw them.** An empty array means passed. Throwing (or returning anything that isn't an array of `{ message, resources?, details?, suggestion? }`) makes the function an `error`, which also fails the check but means "couldn't be evaluated", not "architecture is wrong".
- **Narrow `resource` by its `type`** (`resource?.type === 'domain'` gives a typed Domain) and throw a clear error if the file is in the wrong place.
- **Use the `catalog` read methods** — the same names and arguments as `@eventcatalog/sdk`: `getDomains`, `getServices`, `getEvents`, `getDataStores`, `getTeams`, `getGraph`, `getOwnersForResource`, `getSchemaForMessage`, `getResourcesByProperty`, … Collection getters return `undefined` when there's nothing, so write `(await catalog.getEvents({ latestOnly: true })) ?? []`. There are no write methods, and every call returns a copy, so mutating results is harmless. Full list: [references/api.md](references/api.md).
- **Read collections once and build lookups** (maps or sets) instead of calling the catalog inside loops — e.g. collect every service's `sends` once rather than calling `getProducersAndConsumersForMessage` per message.
- **Reference resources in every violation** with `{ type, id, version? }` — they're linked from the violation in EventCatalog. Types: `domain`, `system`, `service`, `agent`, `event`, `command`, `query`, `flow`, `channel`, `entity`, `container` (`dataStore` is accepted), `data-product`, `adr`, `diagram`, `team`, `user`.
- **Write messages a reader can act on**: name the resources (`Order Service sends Order Amended without a channel`), and put the fix in `suggestion`.
- **Shared logic goes in helper files** (e.g. `fitness/helpers/catalog.ts`) imported with relative paths. Import types with `import type { ReadOnlyCatalog } from '@eventcatalog/fitness'`.
- **Keep them deterministic**: no network calls, no writing files, no dependence on the current date.

Worked examples for common rules (ownership, naming, producers, circular dependencies, domain boundaries, fan-out, channels, schemas, data access): [references/examples.md](references/examples.md).

### Step 5: Run it and verify it

```bash
npx eventcatalog fitness check          # or: npm run fitness
npx eventcatalog fitness check --format json
```

The output lists every function with its result, then the violations and errors under it:

```text
EventCatalog Fitness

✓ Every message has an owner                        Passed
✗ orders / Orders services only use Orders data…    1 violation
    • Order Service writes to payment-database, which is outside the Orders domain
      Resources: order-service (service), payment-database (container)
      Suggestion: Get payment data through the Payments API or its events.
! Every event has a producer                        3 warnings

3 fitness functions evaluated

1 passed
1 warning
1 failed

Report written to .eventcatalog/fitness/results.json
```

Verify before handing over — a rule that can never fail is worse than none:

1. **The new function ran**, with the status you expect. If it shows `Error`, fix it (see Troubleshooting).
2. **Its results are right.** Check a few violations (and a few passes) against the catalog files by hand.
3. **It can fail.** If it passes on the current catalog, confirm it would catch a real violation — reason it through with the user, or break a resource in a scratch copy of the catalog and run the check there. Never leave test edits in the user's catalog.
4. **References resolve.** A reference shown as `(…, not found in the catalog)` has a wrong type or id.

Exit codes: `0` everything passed or only warned, `1` a function failed or couldn't be evaluated, `2` the check itself couldn't run.

### Step 6: Run it in CI

Run `npx eventcatalog fitness check` in the pipeline that checks catalog changes, after installing dependencies. It exits with `1` when a rule fails, which fails the job. GitHub Actions, GitLab and report-publishing examples: [references/ci.md](references/ci.md).

## Troubleshooting

| Symptom                                                                                | Cause / fix                                                                                                                       |
| -------------------------------------------------------------------------------------- | --------------------------------------------------------------------------------------------------------------------------------- |
| Function not listed at all                                                             | File doesn't end in `.fitness.ts`, isn't in a `fitness/` folder, or is under `federated/`, `node_modules`, `dist` or a dot folder |
| `! <file>: Not run: fitness functions must be in a fitness/ folder`                    | Move the file into a `fitness/` folder at the root or directly inside a resource's folder                                         |
| `No resource found for this fitness function: '<folder>' has no index.mdx or index.md` | The `fitness/` folder isn't directly inside a resource's folder                                                                   |
| `… is not in a resource folder (such as domains/ or services/)`                        | The folder holding `fitness/` isn't a recognised resource folder                                                                  |
| `No resource found for this fitness function: '<index>' has no id`                     | The resource's `index.mdx` has no `id` in its frontmatter                                                                         |
| `Could not load the <type> '<id>' (<version>) this fitness function sits alongside`    | The resource beside the `fitness/` folder couldn't be read from the catalog — check its frontmatter is valid                      |
| `The file has no default export`                                                       | Add `export default defineFitnessFunction({ ... })`                                                                               |
| `Invalid fitness function: \`id\` must be a non-empty string, …`                       | Missing `id`/`name`/`run`, or `severity` isn't `'error'` or `'warning'`                                                           |
| `Another fitness function already has the id '<id>'`                                   | Ids must be unique; the first file (by path) runs, later ones error                                                               |
| `run() must return an array of violations …` / `Violation 0: …`                        | `run` returned something else, or a violation is missing `message` or has a bad `resources` entry                                 |
| `Cannot find module './helpers/…'`                                                     | Wrong relative path to a helper — count the folders up to the catalog root                                                        |
| `(…, not found in the catalog)` after a resource                                       | The reference's `type` or `id` doesn't match a resource; it's kept but not linked                                                 |
| `Cannot find module '@eventcatalog/fitness'` in the editor only                        | Install `@eventcatalog/fitness` as a dev dependency; the check itself works without it                                            |
| `fitness` is not a command                                                             | `@eventcatalog/core` is older than 5.3.0 — upgrade, or use `npx @eventcatalog/cli fitness check`                                  |

## Trust

Fitness functions are code from the catalog, run with the same access as any other script in the project. Only run fitness functions you trust, locally or in CI. EventCatalog never runs them in the browser, on page requests, or from federated catalogs.

## Reference Files

- [references/api.md](references/api.md) — `defineFitnessFunction`, the context, every `catalog` method, violations, statuses, the JSON report, CLI options and discovery rules
- [references/examples.md](references/examples.md) — complete fitness functions for common rules, global and scoped, with shared helpers
- [references/ci.md](references/ci.md) — running the check in GitHub Actions and GitLab, publishing the report, reading the JSON output
