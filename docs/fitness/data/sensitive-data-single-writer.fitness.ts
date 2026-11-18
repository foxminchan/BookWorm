import { defineFitnessFunction, type FitnessViolation } from "@eventcatalog/fitness";

type Writer = {
	type: "service" | "agent";
	id: string;
	name: string;
	version: string;
};

export default defineFitnessFunction({
	id: "sensitive-data-single-writer",
	name: "Sensitive data has at most one documented writer",
	description:
		"Confidential and regulated data stores should not be written by multiple services or agents.",
	severity: "warning",

	async run({ catalog }) {
		const [stores, services, agents] = await Promise.all([
			catalog.getDataStores({ latestOnly: true }),
			catalog.getServices({ latestOnly: true }),
			catalog.getAgents({ latestOnly: true }),
		]);
		const participants = [
			...(services ?? []).map((resource) => ({ type: "service" as const, resource })),
			...(agents ?? []).map((resource) => ({ type: "agent" as const, resource })),
		];
		const writersByStore = new Map<string, Writer[]>();

		for (const { type, resource } of participants) {
			const storeIds = new Set((resource.writesTo ?? []).map((store) => store.id));
			for (const storeId of storeIds) {
				const writers = writersByStore.get(storeId) ?? [];
				writers.push({ type, id: resource.id, name: resource.name, version: resource.version });
				writersByStore.set(storeId, writers);
			}
		}

		const violations: FitnessViolation[] = [];
		for (const store of stores ?? []) {
			if (store.classification !== "confidential" && store.classification !== "regulated") {
				continue;
			}
			const writers = writersByStore.get(store.id) ?? [];
			if (writers.length <= 1) continue;

			violations.push({
				message: `${store.name} has ${writers.length} documented writers: ${writers.map((writer) => writer.name).join(", ")}`,
				resources: [
					{ type: "container", id: store.id, version: store.version },
					...writers.map(({ type, id, version }) => ({ type, id, version })),
				],
				suggestion:
					"Review the shared writes and assign one service or agent responsibility for changes where possible.",
			});
		}
		return violations;
	},
});
