import { defineFitnessFunction } from "@eventcatalog/fitness";

export default defineFitnessFunction({
	id: "services-owned-by-teams",
	name: "Every service is owned by a team",
	description:
		"Each service must have an accountable team that remains responsible as individuals change.",
	severity: "error",

	async run({ catalog }) {
		const services = (await catalog.getServices({ latestOnly: true })) ?? [];
		const teams = (await catalog.getTeams()) ?? [];
		const teamIds = new Set(teams.map((team) => team.id));

		return services.flatMap((service) => {
			const teamOwners = (service.owners ?? []).filter((owner) => teamIds.has(owner));
			if (teamOwners.length > 0) return [];

			return [
				{
					message: `${service.name} is not owned by a team`,
					resources: [{ type: "service" as const, id: service.id, version: service.version }],
					details: service.owners?.length
						? `Current owners: ${service.owners.join(", ")}`
						: undefined,
					suggestion: "Add at least one valid team ID to the service's owners field.",
				},
			];
		});
	},
});
