# Fitness functions API reference

Everything a fitness function can use, and everything `eventcatalog fitness check` does with it.

## `defineFitnessFunction`

```ts
import { defineFitnessFunction } from '@eventcatalog/fitness';

export default defineFitnessFunction({
  id: string;                 // required, unique across the catalog (kebab-case by convention)
  name: string;               // required, shown in the output and the report
  description?: string;
  severity?: 'error' | 'warning'; // default 'error'
  run(context: FitnessContext): FitnessViolation[] | Promise<FitnessViolation[]>;
});
```

- It validates the definition (throws on a missing `id`, `name` or `run`, a non-string `description`, or an unknown `severity`) and returns it unchanged, typed.
- Use it as the **default export** of the file. A plain object with the same shape also works, without the type checking.
- `@eventcatalog/fitness` always resolves when the check loads a fitness function, even if the catalog hasn't installed it. Install it as a dev dependency only for editor types.

## The context

```ts
type FitnessContext = {
	resource?: CatalogResource; // the resource the file sits beside; undefined in the root fitness/ folder
	catalog: ReadOnlyCatalog; // read-only SDK for the whole catalog
};
```

### `resource`

The resource whose folder holds the function's `fitness/` folder: its frontmatter, `markdown`, and a `type`. Narrow on `type` to get the right TypeScript type:

```ts
if (resource?.type === "service") {
	resource.sends; // typed as a Service
}
```

| Folder holding `fitness/`                   | `resource.type`             |
| ------------------------------------------- | --------------------------- |
| `domains/*`, `…/subdomains/*`               | `domain`                    |
| `…/systems/*`                               | `system`                    |
| `…/services/*`                              | `service`                   |
| `…/agents/*`                                | `agent`                     |
| `…/events/*`, `…/commands/*`, `…/queries/*` | `event`, `command`, `query` |
| `…/channels/**`                             | `channel`                   |
| `…/flows/**`                                | `flow`                      |
| `…/entities/*`                              | `entity`                    |
| `…/containers/**` (data stores)             | `container`                 |
| `…/data-products/*`                         | `data-product`              |
| `…/adrs/*`                                  | `adr`                       |
| `…/diagrams/**`                             | `diagram`                   |

A `fitness/` folder inside `versioned/<version>/` gets that version of the resource, not the latest.

### `catalog`

The read methods of `@eventcatalog/sdk`, with the same names and arguments. There are no methods that write. Every call returns a copy, so changing a result doesn't affect other calls or other functions.

| Area                      | Methods                                                                                                                                                                                    |
| ------------------------- | ------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------ |
| Events, commands, queries | `getEvent(id, version?)`, `getEvents({ latestOnly?, attachSchema? })`, `eventHasVersion`, `getExamplesFromEvent` (and the same for commands and queries)                                   |
| Messages and schemas      | `getProducersAndConsumersForMessage(id, version?)`, `getSchemaForMessage(id, version?)` → `{ schema, fileName }`, `getMessageBySchemaPath`, `getConsumersOfSchema`, `getProducersOfSchema` |
| Domains                   | `getDomain`, `getDomains`, `domainHasVersion`, `getUbiquitousLanguageFromDomain`                                                                                                           |
| Systems, services, agents | `getSystem(s)`, `systemHasVersion`, `getService(s)`, `serviceHasVersion`, `getSpecificationFilesForService`, `getAgent(s)`, `agentHasVersion`                                              |
| Channels, flows, entities | `getChannel(s)`, `getFlow(s)`, `getEntity`/`getEntities`, and their `…HasVersion`                                                                                                          |
| Data                      | `getDataStore(s)`, `getDataProduct(s)`, and their `…HasVersion`                                                                                                                            |
| ADRs, diagrams            | `getAdr(s)`, `getDiagram(s)`, and their `…HasVersion`                                                                                                                                      |
| People and docs           | `getTeam(s)`, `getUser(s)`, `getOwnersForResource(id, version?)`, `getCustomDoc(s)`, `getChangelog`                                                                                        |
| Relationships and queries | `getGraph(root, options?)`, `getResourcesByProperty(property, options?)`, `getResourcePath`, `getResourceFolderName`, `getEventCatalogConfigurationFile()`                                 |

Notes:

- Collection getters (`getEvents`, `getServices`, …) return `undefined` when there are none: use `?? []`.
- Pass `{ latestOnly: true }` to collection getters unless the rule really is about every version.
- `getGraph({ type, id, version? }, { depth?, flat? })` returns the resources reachable from a resource through its relationships. The root can be any resource type except `diagram`, `team` and `user`. `{ flat: true }` gives `{ root, resources: [{ type, id, version }] }`, where `resources` includes the root itself. `{ depth: 1 }` limits it to direct relationships.
- `attachSchema: true` on message getters adds each message's schema, for rules about schemas.
- `getResourcesByProperty('x-tier', { in: [1, 2], types: ['service'], latestOnly: true })` finds resources by a custom `x-…` frontmatter property. Use `equals` for one value.
- The fields a resource has are its frontmatter: services have `sends`, `receives`, `writesTo`, `readsFrom`, `owners`, `specifications`. Data stores have `container_type`, `classification`, and so on. Read a few `index.mdx` files to see what the catalog actually documents.

## Violations

```ts
type FitnessViolation = {
	message: string; // required
	resources?: { type: string; id: string; version?: string }[];
	details?: string;
	suggestion?: string;
};
```

- **Reference types:** `domain`, `system`, `service`, `agent`, `event`, `command`, `query`, `flow`, `channel`, `entity`, `container`, `data-product`, `adr`, `diagram`, `team` and `user`. Other spellings are accepted and normalised: `dataStore`/`data-store` become `container`, `dataProduct` becomes `data-product`, `subdomain` becomes `domain`, and case doesn't matter.
- **Ids and versions** may be numbers (as frontmatter like `version: 1` gives them). They're recorded as strings.
- **Matching:** each reference is matched to the catalog. If it matches, it's recorded with the version that was evaluated and `resolved: true`, and EventCatalog links to it. If it doesn't match, it's kept with `resolved: false` and shown as `(…, not found in the catalog)`. Unresolved references don't change the result.
- **Rejected output:** anything else makes the function an `error`, naming what's wrong: a non-array, a violation without a non-empty `message`, a resource without `type` and `id`, or a non-string `details` or `suggestion`.

## Results

| Status    | When                                                                                                                                                                                                            |
| --------- | --------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- |
| `passed`  | `run` returned `[]`                                                                                                                                                                                             |
| `warning` | a `warning`-severity function returned violations                                                                                                                                                               |
| `failed`  | an `error`-severity function returned violations                                                                                                                                                                |
| `error`   | the function couldn't be evaluated: it threw, returned something invalid, couldn't be loaded, had no default export, had an invalid definition, reused another function's id, or its resource couldn't be found |
| `not_run` | reserved in the types (printed as "Not run"); the check doesn't produce it today                                                                                                                                |

Functions run one at a time, in path order. One function failing never stops the others.

## The report

Each run writes `.eventcatalog/fitness/results.json` (change it with `--output`):

```json
{
	"schemaVersion": 1,
	"evaluatedAt": "2026-10-19T09:00:00.000Z",
	"catalogRevision": "a38f02c",
	"summary": { "total": 3, "passed": 1, "warnings": 1, "failed": 1, "errors": 0 },
	"functions": [
		{
			"id": "orders-no-shared-data-stores",
			"name": "Orders services only use Orders data stores",
			"severity": "error",
			"status": "failed",
			"source": "domains/Orders/fitness/no-shared-data-stores.fitness.ts",
			"scope": { "type": "domain", "id": "orders", "version": "1.0.0" },
			"violations": [
				{
					"message": "Order Service writes to payment-database, which is outside the Orders domain",
					"resources": [
						{ "type": "service", "id": "order-service", "version": "1.0.0", "resolved": true },
						{ "type": "container", "id": "payment-database", "version": "1.0.0", "resolved": true }
					],
					"suggestion": "Get payment data through the Payments API or its events."
				}
			]
		}
	],
	"diagnostics": []
}
```

- `catalogRevision` is the catalog's git commit, or `null` outside a git repository.
- `scope` is `null` for root functions. `error` results include `error: { message, stack? }`.
- When a file's definition couldn't be read, its `id` falls back to the file's `source` path and its `name` to the file name without `.fitness.ts`.
- `diagnostics` lists `.fitness.ts` files that weren't run because they're outside a `fitness/` folder.
- A catalog with no fitness functions still gets a report (with `total: 0`), so "no rules" can't be mistaken for "no report".

## CLI

```bash
npx eventcatalog fitness check [--output <path>] [--format text|json]
npx @eventcatalog/cli [--dir <catalog>] fitness check [--output <path>] [--format text|json]
```

| Option                  | Default                              |                                                    |
| ----------------------- | ------------------------------------ | -------------------------------------------------- |
| `--output <path>`, `-o` | `.eventcatalog/fitness/results.json` | Where to write the report, relative to the catalog |
| `--format <format>`     | `text`                               | `json` prints the report instead of the summary    |

| Exit code | Meaning                                                                                     |
| --------- | ------------------------------------------------------------------------------------------- |
| `0`       | Every function passed, or only warned                                                       |
| `1`       | A function failed, or couldn't be evaluated                                                 |
| `2`       | The check couldn't run (e.g. the catalog directory doesn't exist, or an unknown `--format`) |

`npx eventcatalog` uses the current directory as the catalog, or `PROJECT_DIR` when it's set. The catalog's `.env` file is loaded first, so fitness functions can read settings from `process.env`.

## Discovery rules

- **Runs:** files ending in `.fitness.ts` (including dot-prefixed ones) that are in a `fitness/` folder, either at the catalog root or directly inside a resource's folder. Folders inside a `fitness/` folder are fine for organising.
- **A resource folder named `fitness`:** the innermost `fitness/` folder that sits in a resource's folder is the one used, so a resource can itself be called `fitness`.
- **Skipped:** `node_modules`, `dist`, dot folders (`.git`, `.eventcatalog`, …), the root `federated/` folder, and symlinks.
- **Not run but reported:** a `.fitness.ts` file outside a `fitness/` folder appears in `diagnostics`.
- **Not federated:** federation doesn't copy `fitness/` folders, so another catalog's rules never run in yours.

## Programmatic use

```ts
import {
	evaluateFitness,
	formatFitnessReport,
	getFitnessExitCode,
	writeFitnessReport,
} from "@eventcatalog/fitness";

const report = await evaluateFitness({ dir: "./my-catalog" });
console.log(formatFitnessReport(report, { color: true }));
writeFitnessReport(report, "./my-catalog/.eventcatalog/fitness/results.json");
process.exitCode = getFitnessExitCode(report);
```

- `runFitnessCheck({ dir, output?, format?, color?, log?, error? })` does everything the CLI does in one call (evaluate, write the report, print) and returns the exit code (`0`, `1` or `2`).
- `evaluateFitness` also takes `catalogRevision` and `evaluatedAt` (a `Date`), for a deterministic report.
- Also exported: `discoverFitnessFiles`, `createReadOnlyCatalog`, `READ_ONLY_METHODS` and `DEFAULT_REPORT_PATH`.
- Every type is exported (`FitnessViolation`, `FitnessContext`, `CatalogResource`, `CatalogResourceType`, `FitnessReport`, …), so helpers can be typed: `const violations: FitnessViolation[] = [];`.
