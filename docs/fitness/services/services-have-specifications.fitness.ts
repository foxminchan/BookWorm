import { defineFitnessFunction, type FitnessViolation } from "@eventcatalog/fitness";

export default defineFitnessFunction({
	id: "services-have-specifications",
	name: "Internal service interfaces have specifications",
	description:
		"Internal services with documented interfaces should list an API or message specification.",
	severity: "warning",

	async run({ catalog }) {
		const [services, systems] = await Promise.all([
			catalog.getServices({ latestOnly: true }),
			catalog.getSystems({ latestOnly: true }),
		]);
		const servicesWithInlineInterfaceDocumentation = new Set([
			"mcp-server",
			"chat-api",
			"catalog-ingestion",
		]);
		const externalServices = new Set(
			(systems ?? [])
				.filter((system) => system.scope === "external")
				.flatMap((system) => (system.services ?? []).map((service) => service.id))
		);
		const violations: FitnessViolation[] = [];

		for (const service of services ?? []) {
			const badges = (service.badges ?? []).map((badge) => badge.content.trim().toLowerCase());
			if (
				service.externalSystem ||
				externalServices.has(service.id) ||
				badges.includes("external")
			) {
				continue;
			}
			if (servicesWithInlineInterfaceDocumentation.has(service.id)) continue;
			if (!service.receives?.length && !badges.includes("internal")) continue;

			const specifications = service.specifications;
			const hasSpecification = Array.isArray(specifications)
				? specifications.some((specification) => specification.path?.trim())
				: specifications &&
					Object.values(specifications).some((path) => typeof path === "string" && path.trim());
			if (hasSpecification) continue;

			violations.push({
				message: `${service.name} has no documented interface specification`,
				resources: [{ type: "service", id: service.id, version: service.version }],
				suggestion:
					"Register an applicable API or message specification, or review how the interface is documented.",
			});
		}
		return violations;
	},
});
