import { defineFitnessFunction, type FitnessViolation } from "@eventcatalog/fitness";
import { referencesLatestMessage } from "../helpers/message-versions";

function connectedGroups(graph: Map<string, Set<string>>, reverse: Map<string, Set<string>>) {
	const visited = new Set<string>();
	const order: string[] = [];
	const visit = (id: string, edges: Map<string, Set<string>>, result: string[]) => {
		const pending = [{ id, expanded: false }];
		while (pending.length > 0) {
			const entry = pending.pop();
			if (!entry) throw new Error("Missing command dependency traversal entry.");
			if (entry.expanded) {
				result.push(entry.id);
				continue;
			}
			if (visited.has(entry.id)) continue;
			visited.add(entry.id);
			pending.push({ id: entry.id, expanded: true });
			for (const next of edges.get(entry.id) ?? []) {
				if (!visited.has(next)) pending.push({ id: next, expanded: false });
			}
		}
	};

	for (const id of [...graph.keys()].sort()) visit(id, graph, order);
	visited.clear();

	const groups: string[][] = [];
	for (const id of order.reverse()) {
		if (visited.has(id)) continue;
		const group: string[] = [];
		visit(id, reverse, group);
		if (group.length > 1) groups.push(group.sort());
	}
	return groups.sort((left, right) => left[0].localeCompare(right[0]));
}

export default defineFitnessFunction({
	id: "no-circular-command-dependencies",
	name: "Services avoid circular command dependencies",
	description: "Warn when services form a cycle through their latest documented commands.",
	severity: "warning",

	async run({ catalog }) {
		const [servicesResult, commandsResult] = await Promise.all([
			catalog.getServices({ latestOnly: true }),
			catalog.getCommands({ latestOnly: true }),
		]);
		const services = servicesResult ?? [];
		const servicesById = new Map(services.map((service) => [service.id, service]));
		const commandsById = new Map((commandsResult ?? []).map((command) => [command.id, command]));
		const senders = new Map<string, Set<string>>();
		const receivers = new Map<string, Set<string>>();
		const graph = new Map(services.map((service) => [service.id, new Set<string>()]));
		const reverse = new Map(services.map((service) => [service.id, new Set<string>()]));

		for (const service of services) {
			for (const [references, participants] of [
				[service.sends ?? [], senders],
				[service.receives ?? [], receivers],
			] as const) {
				for (const reference of references) {
					const command = commandsById.get(reference.id);
					if (!command || !referencesLatestMessage(reference, command)) continue;
					const ids = participants.get(command.id) ?? new Set<string>();
					ids.add(service.id);
					participants.set(command.id, ids);
				}
			}
		}

		for (const [commandId, senderIds] of senders) {
			for (const sender of senderIds) {
				for (const receiver of receivers.get(commandId) ?? []) {
					if (sender === receiver) continue;
					const outgoing = graph.get(sender);
					const incoming = reverse.get(receiver);
					if (!outgoing || !incoming) {
						throw new Error(`Unknown service in command dependency ${sender} -> ${receiver}.`);
					}
					outgoing.add(receiver);
					incoming.add(sender);
				}
			}
		}

		const violations: FitnessViolation[] = [];
		for (const group of connectedGroups(graph, reverse)) {
			const members = group.map((id) => {
				const service = servicesById.get(id);
				if (!service) throw new Error(`Unknown service "${id}" in command dependency cycle.`);
				return service;
			});
			violations.push({
				message: `Services depend on each other through commands: ${members.map((service) => service.name).join(", ")}`,
				resources: members.map((service) => ({
					type: "service",
					id: service.id,
					version: service.version,
				})),
				suggestion: "Review the command directions and break the cycle where appropriate.",
			});
		}
		return violations;
	},
});
