import { defineFitnessFunction, type FitnessViolation } from "@eventcatalog/fitness";
import { referencesLatestMessage } from "../helpers/message-versions";

export default defineFitnessFunction({
	id: "events-have-producers",
	name: "Latest events have documented producers",
	description: "Each latest event version should be sent by a service, agent, or domain.",
	severity: "warning",

	async run({ catalog }) {
		const [events, services, agents, domains] = await Promise.all([
			catalog.getEvents({ latestOnly: true }),
			catalog.getServices({ latestOnly: true }),
			catalog.getAgents({ latestOnly: true }),
			catalog.getDomains({ latestOnly: true }),
		]);
		const latestEvents = events ?? [];
		const eventsById = new Map(latestEvents.map((event) => [event.id, event]));
		const producedIds = new Set<string>();

		for (const participant of [...(services ?? []), ...(agents ?? []), ...(domains ?? [])]) {
			for (const reference of participant.sends ?? []) {
				const event = eventsById.get(reference.id);
				if (event && referencesLatestMessage(reference, event)) producedIds.add(event.id);
			}
		}

		const violations: FitnessViolation[] = [];
		for (const event of latestEvents) {
			if (producedIds.has(event.id)) continue;
			violations.push({
				message: `${event.name} has no documented producer for version ${event.version}`,
				resources: [{ type: "event", id: event.id, version: event.version }],
				suggestion:
					"Add this event version to the producer's sends field, or review whether the event is still used.",
			});
		}
		return violations;
	},
});
