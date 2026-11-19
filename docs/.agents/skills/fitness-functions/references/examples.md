# Fitness function examples

Complete fitness functions for common architecture rules. Each one has been run against EventCatalog's example catalog. Adapt the ids, names, folders and thresholds to the user's catalog, and check the catalog documents the fields a rule reads (`owners`, `writesTo`, `sends.to`, …) before relying on it.

- [Shared helpers](#shared-helpers)
- [Every message has an owner](#every-message-has-an-owner) — global, ownership
- [Owners are known teams or users](#owners-are-known-teams-or-users) — global, cross-references
- [Message ids follow a naming convention](#message-ids-follow-a-naming-convention) — global, uses a helper
- [Every message has a producer](#every-message-has-a-producer) — global warning, reads collections once
- [No circular dependencies between services](#no-circular-dependencies-between-services) — global, dependency graph
- [Confidential data has a single writer](#confidential-data-has-a-single-writer) — global, data ownership
- [A domain's services only use its own data stores](#a-domains-services-only-use-its-own-data-stores) — scoped to a domain
- [A system's services depend on few others](#a-systems-services-depend-on-few-others) — scoped to a system
- [A service publishes its events to a channel](#a-service-publishes-its-events-to-a-channel) — scoped to a service
- [An event's schema requires the fields consumers rely on](#an-events-schema-requires-the-fields-consumers-rely-on) — scoped to an event
- [Only one domain may access a data store](#only-one-domain-may-access-a-data-store) — scoped to a data store

## Shared helpers

Helpers live beside the fitness functions. Only `*.fitness.ts` files run, so helper files are safe to import.

```ts
// fitness/helpers/catalog.ts
import type { ReadOnlyCatalog } from "@eventcatalog/fitness";

/** The ids of the services in a domain: its own services, its systems' services, and those of its subdomains. */
export const getServiceIdsInDomain = async (
	catalog: ReadOnlyCatalog,
	domainId: string
): Promise<Set<string>> => {
	const domain = await catalog.getDomain(domainId);
	const ids = new Set<string>((domain?.services ?? []).map((service) => service.id));

	for (const pointer of domain?.systems ?? []) {
		const system = await catalog.getSystem(pointer.id, pointer.version);
		for (const service of system?.services ?? []) ids.add(service.id);
	}

	for (const subdomain of domain?.domains ?? []) {
		for (const id of await getServiceIdsInDomain(catalog, subdomain.id)) ids.add(id);
	}

	return ids;
};

/** The ids of the data stores (containers) that belong to a domain's systems. */
export const getDataStoreIdsInDomain = async (
	catalog: ReadOnlyCatalog,
	domainId: string
): Promise<Set<string>> => {
	const domain = await catalog.getDomain(domainId);
	const ids = new Set<string>();

	for (const pointer of domain?.systems ?? []) {
		const system = await catalog.getSystem(pointer.id, pointer.version);
		for (const container of system?.containers ?? []) ids.add(container.id);
	}

	return ids;
};

/** Which services each service depends on: A depends on B when A receives a message B sends. */
export const getServiceDependencies = async (
	catalog: ReadOnlyCatalog
): Promise<Map<string, Set<string>>> => {
	const services = (await catalog.getServices({ latestOnly: true })) ?? [];

	const producersOf = new Map<string, Set<string>>();
	for (const service of services) {
		for (const message of service.sends ?? []) {
			if (!producersOf.has(message.id)) producersOf.set(message.id, new Set());
			producersOf.get(message.id)!.add(service.id);
		}
	}

	const dependencies = new Map<string, Set<string>>();
	for (const service of services) {
		const dependsOn = new Set<string>();
		for (const message of service.receives ?? []) {
			for (const producer of producersOf.get(message.id) ?? []) {
				if (producer !== service.id) dependsOn.add(producer);
			}
		}
		dependencies.set(service.id, dependsOn);
	}

	return dependencies;
};
```

```ts
// fitness/helpers/naming.ts
/** `order-created` is kebab-case; `OrderCreated`, `order_created` and `orderCreated` are not. */
export const isKebabCase = (value: string) => /^[a-z0-9]+(-[a-z0-9]+)*$/.test(value);

export const toKebabCase = (value: string) =>
	value
		.replace(/([a-z0-9])([A-Z])/g, "$1-$2")
		.replace(/[_\s.]+/g, "-")
		.toLowerCase();
```

Import a helper with a relative path from the fitness function's file, counting folders up to the catalog root: `'../helpers/catalog'` from `fitness/services/`, `'../../../fitness/helpers/catalog'` from `domains/Orders/fitness/`.

## Every message has an owner

```ts
// fitness/messages-have-owners.fitness.ts
import { defineFitnessFunction } from "@eventcatalog/fitness";

export default defineFitnessFunction({
	id: "messages-have-owners",
	name: "Every message has an owner",
	description:
		"Someone has to be accountable for every message, so consumers know who to talk to about changes.",
	severity: "error",

	async run({ catalog }) {
		const messages = [
			...((await catalog.getEvents({ latestOnly: true })) ?? []).map((message) => ({
				type: "event",
				message,
			})),
			...((await catalog.getCommands({ latestOnly: true })) ?? []).map((message) => ({
				type: "command",
				message,
			})),
			...((await catalog.getQueries({ latestOnly: true })) ?? []).map((message) => ({
				type: "query",
				message,
			})),
		];

		return messages
			.filter(({ message }) => !message.owners || message.owners.length === 0)
			.map(({ type, message }) => ({
				message: `The ${type} ${message.name} has no owners`,
				resources: [{ type, id: message.id, version: message.version }],
				suggestion: `Add the team or user responsible for the ${type} to its \`owners\`.`,
			}));
	},
});
```

## Owners are known teams or users

```ts
// fitness/owners-exist.fitness.ts
import { defineFitnessFunction } from "@eventcatalog/fitness";

export default defineFitnessFunction({
	id: "owners-exist",
	name: "Owners are known teams or users",
	description:
		"An owner that is not in the catalog cannot be contacted, so the resource is effectively unowned.",

	async run({ catalog }) {
		const teams = (await catalog.getTeams()) ?? [];
		const users = (await catalog.getUsers()) ?? [];
		const knownOwners = new Set([...teams, ...users].map((owner) => owner.id));

		const resourcesByType = {
			domain: (await catalog.getDomains({ latestOnly: true })) ?? [],
			service: (await catalog.getServices({ latestOnly: true })) ?? [],
			event: (await catalog.getEvents({ latestOnly: true })) ?? [],
			command: (await catalog.getCommands({ latestOnly: true })) ?? [],
			query: (await catalog.getQueries({ latestOnly: true })) ?? [],
		};

		const violations = [];
		for (const [type, resources] of Object.entries(resourcesByType)) {
			for (const resource of resources) {
				for (const owner of resource.owners ?? []) {
					// Owners can be written as an id, or as an object with an id
					const ownerId = typeof owner === "string" ? owner : (owner as { id: string }).id;
					if (knownOwners.has(ownerId)) continue;

					violations.push({
						message: `${resource.name} is owned by '${ownerId}', which is not a team or user in the catalog`,
						resources: [{ type, id: resource.id, version: resource.version }],
						suggestion: `Add '${ownerId}' to teams/ or users/, or fix the owner's id.`,
					});
				}
			}
		}

		return violations;
	},
});
```

## Message ids follow a naming convention

```ts
// fitness/messages/message-ids-are-kebab-case.fitness.ts
import { defineFitnessFunction } from "@eventcatalog/fitness";
import { isKebabCase, toKebabCase } from "../helpers/naming";

export default defineFitnessFunction({
	id: "message-ids-are-kebab-case",
	name: "Message ids are kebab-case",

	async run({ catalog }) {
		const messages = [
			...((await catalog.getEvents({ latestOnly: true })) ?? []).map((message) => ({
				type: "event",
				message,
			})),
			...((await catalog.getCommands({ latestOnly: true })) ?? []).map((message) => ({
				type: "command",
				message,
			})),
			...((await catalog.getQueries({ latestOnly: true })) ?? []).map((message) => ({
				type: "query",
				message,
			})),
		];

		return messages
			.filter(({ message }) => !isKebabCase(message.id))
			.map(({ type, message }) => ({
				message: `The ${type} id '${message.id}' is not kebab-case`,
				resources: [{ type, id: message.id, version: message.version }],
				suggestion: `Rename it to '${toKebabCase(message.id)}'.`,
			}));
	},
});
```

## Every message has a producer

Reads every service, agent and domain once and builds a set, rather than calling `getProducersAndConsumersForMessage` for each message (which re-reads the whole catalog on every call).

```ts
// fitness/messages/messages-have-producers.fitness.ts
import { defineFitnessFunction } from "@eventcatalog/fitness";

export default defineFitnessFunction({
	id: "messages-have-producers",
	name: "Every message has a producer",
	severity: "warning",

	async run({ catalog }) {
		const participants = [
			...((await catalog.getServices({ latestOnly: true })) ?? []),
			...((await catalog.getAgents({ latestOnly: true })) ?? []),
			...((await catalog.getDomains({ latestOnly: true })) ?? []),
		];
		const sent = new Set(
			participants.flatMap((participant) => (participant.sends ?? []).map((message) => message.id))
		);
		const receiversOf = (messageId: string) =>
			participants
				.filter((participant) => participant.receives?.some((message) => message.id === messageId))
				.map(({ id }) => id);

		const events = (await catalog.getEvents({ latestOnly: true })) ?? [];
		const commands = (await catalog.getCommands({ latestOnly: true })) ?? [];

		const violations = [];
		for (const [type, messages] of [
			["event", events],
			["command", commands],
		] as const) {
			for (const message of messages) {
				if (sent.has(message.id)) continue;

				const consumers = receiversOf(message.id);
				violations.push({
					message: `No one sends the ${type} ${message.name}`,
					resources: [{ type, id: message.id, version: message.version }],
					details:
						consumers.length > 0
							? `It is received by ${consumers.join(", ")}.`
							: "It is not received by anything either.",
					suggestion: `Add the ${type} to the \`sends\` of the service that produces it, or remove it if it is no longer used.`,
				});
			}
		}

		return violations;
	},
});
```

## No circular dependencies between services

```ts
// fitness/services/no-circular-service-dependencies.fitness.ts
import { defineFitnessFunction } from "@eventcatalog/fitness";
import { getServiceDependencies } from "../helpers/catalog";

/** Every cycle in the graph, each starting from its alphabetically first service so it is only reported once. */
const findCycles = (graph: Map<string, Set<string>>): string[][] => {
	const cycles: string[][] = [];
	const seen = new Set<string>();

	const visit = (start: string, current: string, path: string[]) => {
		for (const next of graph.get(current) ?? []) {
			if (next === start) {
				const key = [...path].sort().join(",");
				if (!seen.has(key)) {
					seen.add(key);
					cycles.push([...path, start]);
				}
			} else if (next > start && !path.includes(next)) {
				visit(start, next, [...path, next]);
			}
		}
	};

	for (const service of [...graph.keys()].sort()) visit(service, service, [service]);
	return cycles;
};

export default defineFitnessFunction({
	id: "no-circular-service-dependencies",
	name: "No circular dependencies between services",

	async run({ catalog }) {
		const dependencies = await getServiceDependencies(catalog);

		return findCycles(dependencies).map((cycle) => ({
			message: `Circular dependency: ${cycle.join(" → ")}`,
			resources: cycle.slice(0, -1).map((id) => ({ type: "service", id })),
			suggestion:
				"Break the cycle, for example by having one side react to events instead of the other calling it.",
		}));
	},
});
```

## Confidential data has a single writer

Uses a data store field (`classification`) to decide which stores the rule applies to. The same approach works with custom `x-…` properties and `catalog.getResourcesByProperty`.

```ts
// fitness/data/confidential-data-has-one-writer.fitness.ts
import { defineFitnessFunction } from "@eventcatalog/fitness";

const SENSITIVE = ["confidential", "regulated"];

export default defineFitnessFunction({
	id: "confidential-data-has-one-writer",
	name: "Confidential data has a single writer",
	description: "Sensitive data is easier to protect and audit when only one service can change it.",

	async run({ catalog }) {
		const dataStores = (await catalog.getDataStores({ latestOnly: true })) ?? [];
		const services = (await catalog.getServices({ latestOnly: true })) ?? [];

		return dataStores
			.filter((dataStore) => SENSITIVE.includes(String(dataStore.classification)))
			.map((dataStore) => ({
				dataStore,
				writers: services.filter((service) =>
					service.writesTo?.some((pointer) => pointer.id === dataStore.id)
				),
			}))
			.filter(({ writers }) => writers.length > 1)
			.map(({ dataStore, writers }) => ({
				message: `${dataStore.name} (${dataStore.classification}) is written to by ${writers.length} services: ${writers.map((service) => service.name).join(", ")}`,
				resources: [
					{ type: "container", id: dataStore.id, version: dataStore.version },
					...writers.map((service) => ({
						type: "service",
						id: service.id,
						version: service.version,
					})),
				],
				suggestion: "Choose one service to own writes, and have the others ask it to make changes.",
			}));
	},
});
```

## A domain's services only use its own data stores

Placed in the domain's folder, so `resource` is the domain.

```ts
// domains/Ordering/fitness/no-shared-data-stores.fitness.ts
import { defineFitnessFunction } from "@eventcatalog/fitness";
import { getDataStoreIdsInDomain, getServiceIdsInDomain } from "../../../fitness/helpers/catalog";

export default defineFitnessFunction({
	id: "ordering-no-shared-data-stores",
	name: "Services only use their own domain’s data stores",

	async run({ resource, catalog }) {
		if (resource?.type !== "domain") throw new Error("Expected to run alongside a domain");

		const serviceIds = await getServiceIdsInDomain(catalog, resource.id);
		const ownDataStores = await getDataStoreIdsInDomain(catalog, resource.id);

		const violations = [];
		for (const serviceId of serviceIds) {
			const service = await catalog.getService(serviceId);
			if (!service) continue;

			const used = [
				...(service.writesTo ?? []).map((pointer) => ({ ...pointer, access: "writes to" })),
				...(service.readsFrom ?? []).map((pointer) => ({ ...pointer, access: "reads from" })),
			];

			for (const dataStore of used.filter((pointer) => !ownDataStores.has(pointer.id))) {
				violations.push({
					message: `${service.name} ${dataStore.access} ${dataStore.id}, which is outside the ${resource.name} domain`,
					resources: [
						{ type: "service", id: service.id, version: service.version },
						{ type: "container", id: dataStore.id },
					],
					suggestion: `Get the data from the domain that owns ${dataStore.id} through its events or API.`,
				});
			}
		}

		return violations;
	},
});
```

## A system's services depend on few others

```ts
// domains/Ordering/systems/checkout-system/fitness/service-fan-out.fitness.ts
import { defineFitnessFunction } from "@eventcatalog/fitness";
import { getServiceDependencies } from "../../../../../fitness/helpers/catalog";

const MAX_DEPENDENCIES = 3;

export default defineFitnessFunction({
	id: "checkout-service-fan-out",
	name: `Services depend on at most ${MAX_DEPENDENCIES} others`,
	severity: "warning",

	async run({ resource, catalog }) {
		if (resource?.type !== "system") throw new Error("Expected to run alongside a system");

		const dependencies = await getServiceDependencies(catalog);

		return (resource.services ?? [])
			.map((service) => ({
				id: service.id,
				dependsOn: [...(dependencies.get(service.id) ?? [])].sort(),
			}))
			.filter(({ dependsOn }) => dependsOn.length > MAX_DEPENDENCIES)
			.map(({ id, dependsOn }) => ({
				message: `${id} depends on ${dependsOn.length} services`,
				resources: [
					{ type: "service", id },
					...dependsOn.map((dependency) => ({ type: "service", id: dependency })),
				],
				details: `It receives messages from ${dependsOn.join(", ")}.`,
				suggestion:
					"Consider whether some of these could be combined, or moved off the critical path.",
			}));
	},
});
```

## A service publishes its events to a channel

```ts
// …/services/OrderService/fitness/events-published-to-a-channel.fitness.ts
import { defineFitnessFunction } from "@eventcatalog/fitness";

export default defineFitnessFunction({
	id: "order-service-events-published-to-a-channel",
	name: "Events are published to a channel",

	async run({ resource, catalog }) {
		if (resource?.type !== "service") throw new Error("Expected to run alongside a service");

		const violations = [];
		for (const sent of resource.sends ?? []) {
			// Commands and queries can be sent point to point; only events need a channel
			const event = await catalog.getEvent(sent.id, sent.version);
			if (!event) continue;
			if (sent.to && sent.to.length > 0) continue;

			violations.push({
				message: `${resource.name} sends ${event.name} without a channel`,
				resources: [
					{ type: "service", id: resource.id, version: resource.version },
					{ type: "event", id: event.id, version: event.version },
				],
				suggestion:
					"Add the channel the event is published to, e.g. `to: [{ id: orders.events }]`.",
			});
		}

		return violations;
	},
});
```

## An event's schema requires the fields consumers rely on

```ts
// …/events/OrderCreated/fitness/schema-has-required-fields.fitness.ts
import { defineFitnessFunction } from "@eventcatalog/fitness";

const REQUIRED_FIELDS = ["orderId", "customerId", "total", "currency"];

export default defineFitnessFunction({
	id: "order-created-schema-has-required-fields",
	name: "Schema requires the fields consumers rely on",

	async run({ resource, catalog }) {
		if (resource?.type !== "event") throw new Error("Expected to run alongside an event");
		const event = { type: "event", id: resource.id, version: resource.version };

		const file = await catalog.getSchemaForMessage(resource.id, resource.version);
		if (!file) {
			return [
				{
					message: `${resource.name} has no schema`,
					resources: [event],
					suggestion: "Add a JSON schema and set `schemaPath`.",
				},
			];
		}

		const schema = JSON.parse(file.schema) as {
			properties?: Record<string, unknown>;
			required?: string[];
		};
		const properties = Object.keys(schema.properties ?? {});
		const required = schema.required ?? [];

		return REQUIRED_FIELDS.filter(
			(field) => !properties.includes(field) || !required.includes(field)
		).map((field) => ({
			message: properties.includes(field)
				? `${resource.name} defines '${field}' but doesn't require it`
				: `${resource.name} has no '${field}' field`,
			resources: [event],
			details: `Checked ${file.fileName}`,
			suggestion: `Add '${field}' to the schema's properties and to \`required\`.`,
		}));
	},
});
```

To check an older version of the same event, put a fitness function in `events/OrderCreated/versioned/0.6.0/fitness/` — `resource` is then version `0.6.0`.

## Only one domain may access a data store

```ts
// domains/Payments/systems/payment-processing-system/containers/payment-database/fitness/pci-access.fitness.ts
import { defineFitnessFunction } from "@eventcatalog/fitness";
import { getServiceIdsInDomain } from "../../../../../../../fitness/helpers/catalog";

export default defineFitnessFunction({
	id: "payment-database-pci-access",
	name: "Only Payments services can access card data",
	description: "Keeps the Payment Database inside the PCI boundary: the Payments domain.",

	async run({ resource, catalog }) {
		if (resource?.type !== "container") throw new Error("Expected to run alongside a data store");

		const paymentsServices = await getServiceIdsInDomain(catalog, "payments");
		const services = (await catalog.getServices({ latestOnly: true })) ?? [];

		return services
			.filter((service) =>
				[...(service.readsFrom ?? []), ...(service.writesTo ?? [])].some(
					(pointer) => pointer.id === resource.id
				)
			)
			.filter((service) => !paymentsServices.has(service.id))
			.map((service) => ({
				message: `${service.name} accesses ${resource.name} but is not in the Payments domain`,
				resources: [
					{ type: "service", id: service.id, version: service.version },
					{ type: "container", id: resource.id, version: resource.version },
				],
				suggestion: "Call the Payment API instead of reading the database directly.",
			}));
	},
});
```
