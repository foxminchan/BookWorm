import { defineFitnessFunction, type FitnessViolation } from "@eventcatalog/fitness";

export default defineFitnessFunction({
	id: "messages-have-valid-owners",
	name: "Messages have known owners",
	description: "Each event, command, and query must name existing teams or users as owners.",
	severity: "error",

	async run({ catalog }) {
		const [events, commands, queries, teams, users] = await Promise.all([
			catalog.getEvents({ latestOnly: true }),
			catalog.getCommands({ latestOnly: true }),
			catalog.getQueries({ latestOnly: true }),
			catalog.getTeams(),
			catalog.getUsers(),
		]);
		const knownOwners = new Set([...(teams ?? []), ...(users ?? [])].map((owner) => owner.id));
		const messages = [
			...(events ?? []).map((message) => ({ type: "event", message })),
			...(commands ?? []).map((message) => ({ type: "command", message })),
			...(queries ?? []).map((message) => ({ type: "query", message })),
		];
		const violations: FitnessViolation[] = [];

		for (const { type, message } of messages) {
			const resources = [{ type, id: message.id, version: message.version }];
			if (!message.owners?.length) {
				violations.push({
					message: `${message.name} has no owner`,
					resources,
					suggestion: "Add a team or user ID to the message's owners field.",
				});
				continue;
			}

			const unknownOwners = message.owners.filter((owner) => !knownOwners.has(owner));
			if (unknownOwners.length > 0) {
				violations.push({
					message: `${message.name} has unknown owners: ${unknownOwners.join(", ")}`,
					resources,
					suggestion: "Correct the owner IDs or add the missing teams or users to the catalog.",
				});
			}
		}

		return violations;
	},
});
