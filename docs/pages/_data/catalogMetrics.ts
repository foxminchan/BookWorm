import {
	getAdrs,
	getAgents,
	getChannels,
	getCommands,
	getContainers,
	getDomains,
	getEntities,
	getEvents,
	getFlows,
	getQueries,
	getServices,
	getSystems,
	getTeams,
	getUsers,
} from "@catalog/utils";
import { SERVICE_TEST_MATRIX, type ServiceTestMapping } from "./reports";

export type ResourceRef = {
	id: string;
	version?: string;
	collection?: string;
};

export type Badge = {
	content: string;
};

export type SpecificationItem = {
	type?: string;
	path?: string;
	name?: string;
};

export type CatalogEntry<T = Record<string, unknown>> = {
	id: string;
	data: {
		id: string;
		name: string;
		version: string;
		summary?: string;
		owners?: Array<string | ResourceRef>;
		badges?: Badge[];
		sends?: ResourceRef[];
		receives?: ResourceRef[];
		writesTo?: ResourceRef[];
		readsFrom?: ResourceRef[];
		services?: ResourceRef[];
		specifications?: SpecificationItem[] | Record<string, string>;
		classification?: string;
		scope?: string;
		externalSystem?: boolean;
		repository?: {
			language?: string;
			url?: string;
		};
	} & T;
};

export type TeamEntry = {
	data: {
		id: string;
		name: string;
		summary?: string;
		members?: Array<string | ResourceRef>;
	};
};

export type FitnessCheckSummary = {
	id: string;
	name: string;
	category: string;
	severity: "error" | "warning";
	description: string;
	totalChecked: number;
	passedCount: number;
	violations: string[];
};

export type MessageBreakdownItem = {
	label: string;
	shortLabel: string;
	count: number;
	percent: number;
	colorClass: string;
	borderClass: string;
	textClass: string;
	href: string;
	description: string;
};

export type VerificationLayer = {
	tier: string;
	count: number;
	unit: string;
	tools: string;
	workflow: string;
	widthPercent: number;
	barColor: string;
};

export type TeamOwnershipStat = {
	id: string;
	name: string;
	summary?: string;
	servicesCount: number;
	messagesCount: number;
	domainsCount: number;
	containersCount: number;
	totalAssets: number;
};

export type EnrichedService = {
	id: string;
	name: string;
	version: string;
	summary?: string;
	isExternal: boolean;
	specs: string[];
	sendsCount: number;
	receivesCount: number;
	totalMessages: number;
	storesCount: number;
	readsCount: number;
	writesCount: number;
	owners: { id: string; name: string }[];
	testMapping?: Omit<ServiceTestMapping, "serviceId">;
	language: string;
};

const INLINE_DOC_SERVICES = new Set(["mcp-server", "chat-api", "catalog-ingestion"]);

function extractOwnerId(owner: string | ResourceRef): string {
	return typeof owner === "string" ? owner : owner.id;
}

function getServiceSpecLabels(service: CatalogEntry): string[] {
	const specs = service.data.specifications;
	if (!specs) return [];
	if (Array.isArray(specs)) {
		return specs
			.filter((s) => s.path?.trim())
			.map((s) => (s.name || s.type || "Spec").toUpperCase());
	}
	return Object.entries(specs)
		.filter(([, path]) => typeof path === "string" && path.trim())
		.map(([key]) => key.toUpperCase());
}

function countCircularCommandDependencies(
	services: CatalogEntry[],
	commands: CatalogEntry[]
): number {
	const commandIds = new Set(commands.map((c) => c.data.id));
	const cmdSenders = new Map<string, Set<string>>();
	const cmdReceivers = new Map<string, Set<string>>();

	for (const svc of services) {
		for (const ref of svc.data.sends ?? []) {
			if (!commandIds.has(ref.id)) continue;
			const s = cmdSenders.get(ref.id) ?? new Set<string>();
			s.add(svc.data.id);
			cmdSenders.set(ref.id, s);
		}
		for (const ref of svc.data.receives ?? []) {
			if (!commandIds.has(ref.id)) continue;
			const r = cmdReceivers.get(ref.id) ?? new Set<string>();
			r.add(svc.data.id);
			cmdReceivers.set(ref.id, r);
		}
	}

	const serviceEdges = new Map<string, Set<string>>(
		services.map((s) => [s.data.id, new Set<string>()])
	);
	for (const [cmdId, senders] of cmdSenders) {
		for (const sender of senders) {
			for (const receiver of cmdReceivers.get(cmdId) ?? []) {
				if (sender !== receiver) {
					serviceEdges.get(sender)?.add(receiver);
				}
			}
		}
	}

	let circularCommandCount = 0;
	for (const [svcA, targets] of serviceEdges) {
		for (const svcB of targets) {
			if (serviceEdges.get(svcB)?.has(svcA) && svcA < svcB) {
				circularCommandCount++;
			}
		}
	}
	return circularCommandCount;
}

export async function loadCatalogDashboardData(base: string) {
	const [
		domains,
		systems,
		services,
		events,
		commands,
		queries,
		flows,
		channels,
		entities,
		agents,
		containers,
		adrs,
		teams,
		users,
	] = await Promise.all([
		getDomains({ getAllVersions: false }) as Promise<CatalogEntry[]>,
		getSystems({ getAllVersions: false }) as Promise<CatalogEntry[]>,
		getServices({ getAllVersions: false }) as Promise<CatalogEntry[]>,
		getEvents({ getAllVersions: false }) as Promise<CatalogEntry[]>,
		getCommands({ getAllVersions: false }) as Promise<CatalogEntry[]>,
		getQueries({ getAllVersions: false }) as Promise<CatalogEntry[]>,
		getFlows({ getAllVersions: false }) as Promise<CatalogEntry[]>,
		getChannels({ getAllVersions: false }) as Promise<CatalogEntry[]>,
		getEntities({ getAllVersions: false }) as Promise<CatalogEntry[]>,
		getAgents({ getAllVersions: false }) as Promise<CatalogEntry[]>,
		getContainers({ getAllVersions: false }) as Promise<CatalogEntry[]>,
		getAdrs({ getAllVersions: false }) as Promise<CatalogEntry[]>,
		getTeams() as Promise<TeamEntry[]>,
		getUsers() as Promise<TeamEntry[]>,
	]);

	const teamMap = new Map(teams.map((t) => [t.data.id, t.data.name]));
	const userMap = new Map(users.map((u) => [u.data.id, u.data.name]));
	const knownOwnerIds = new Set([...teamMap.keys(), ...userMap.keys()]);
	const teamIds = new Set(teamMap.keys());

	const externalSystemServiceIds = new Set(
		systems
			.filter((s) => s.data.scope === "external")
			.flatMap((s) => (s.data.services ?? []).map((svc) => svc.id))
	);

	const isExternalService = (service: CatalogEntry): boolean => {
		const badges = (service.data.badges ?? []).map((b) => b.content.trim().toLowerCase());
		return Boolean(
			service.data.externalSystem ||
			externalSystemServiceIds.has(service.data.id) ||
			badges.includes("external")
		);
	};

	// 1. Architecture Fitness Functions
	const servicesWithoutTeamOwner = services.filter((svc) => {
		const owners = (svc.data.owners ?? []).map(extractOwnerId);
		return !owners.some((id) => teamIds.has(id));
	});

	const allMessages = [
		...events.map((m) => ({ type: "Event", entry: m })),
		...commands.map((m) => ({ type: "Command", entry: m })),
		...queries.map((m) => ({ type: "Query", entry: m })),
	];

	const messagesWithInvalidOwners = allMessages.filter(({ entry }) => {
		const owners = (entry.data.owners ?? []).map(extractOwnerId);
		return owners.length === 0 || owners.some((id) => !knownOwnerIds.has(id));
	});

	const servicesMissingSpecs = services.filter((svc) => {
		if (isExternalService(svc)) return false;
		if (INLINE_DOC_SERVICES.has(svc.data.id)) return false;
		const badges = (svc.data.badges ?? []).map((b) => b.content.trim().toLowerCase());
		if (!svc.data.receives?.length && !badges.includes("internal")) return false;
		return getServiceSpecLabels(svc).length === 0;
	});

	const producedEventIds = new Set<string>();
	for (const participant of [...services, ...agents, ...domains]) {
		for (const ref of participant.data.sends ?? []) {
			producedEventIds.add(ref.id);
		}
	}
	const orphanedEvents = events.filter((ev) => !producedEventIds.has(ev.data.id));

	const writersByStore = new Map<string, string[]>();
	for (const participant of [...services, ...agents]) {
		const storeIds = new Set((participant.data.writesTo ?? []).map((s) => s.id));
		for (const storeId of storeIds) {
			const list = writersByStore.get(storeId) ?? [];
			list.push(participant.data.name);
			writersByStore.set(storeId, list);
		}
	}
	const multiWriterSensitiveStores = containers.filter((store) => {
		const cls = store.data.classification;
		if (cls !== "confidential" && cls !== "regulated") return false;
		return (writersByStore.get(store.data.id) ?? []).length > 1;
	});

	const circularCommandCount = countCircularCommandDependencies(services, commands);
	const internalServicesCount = services.filter((s) => !isExternalService(s)).length;
	const regulatedStoresCount = containers.filter(
		(c) => c.data.classification === "confidential" || c.data.classification === "regulated"
	).length;

	const fitnessChecks: FitnessCheckSummary[] = [
		{
			id: "services-owned-by-teams",
			name: "Every service is owned by a team",
			category: "Ownership",
			severity: "error",
			description: "Each service must have an accountable team owner in the catalog.",
			totalChecked: services.length,
			passedCount: services.length - servicesWithoutTeamOwner.length,
			violations: servicesWithoutTeamOwner.map((s) => s.data.name),
		},
		{
			id: "messages-have-valid-owners",
			name: "Messages have known owners",
			category: "Ownership",
			severity: "error",
			description: "Every event, command, and query must be owned by an existing team or user.",
			totalChecked: allMessages.length,
			passedCount: allMessages.length - messagesWithInvalidOwners.length,
			violations: messagesWithInvalidOwners.map((m) => m.entry.data.name),
		},
		{
			id: "services-have-specifications",
			name: "Internal service interfaces have specifications",
			category: "Contracts",
			severity: "warning",
			description:
				"Internal services expose OpenAPI, AsyncAPI, Protobuf, or inline MCP/AI contract specifications.",
			totalChecked: internalServicesCount,
			passedCount: internalServicesCount - servicesMissingSpecs.length,
			violations: servicesMissingSpecs.map((s) => s.data.name),
		},
		{
			id: "events-have-producers",
			name: "Latest events have documented producers",
			category: "Messaging",
			severity: "warning",
			description: "Every integration event is emitted by a documented service, agent, or domain.",
			totalChecked: events.length,
			passedCount: events.length - orphanedEvents.length,
			violations: orphanedEvents.map((e) => e.data.name),
		},
		{
			id: "no-circular-command-dependencies",
			name: "Services avoid circular command dependencies",
			category: "Architecture",
			severity: "warning",
			description:
				"Services must not form bidirectional cycles through synchronous or async commands.",
			totalChecked: services.length,
			passedCount: services.length - circularCommandCount,
			violations: [],
		},
		{
			id: "sensitive-data-single-writer",
			name: "Sensitive data has at most one writer",
			category: "Data Governance",
			severity: "warning",
			description:
				"Confidential and regulated data stores enforce single-writer microservice ownership.",
			totalChecked: regulatedStoresCount || containers.length,
			passedCount: (regulatedStoresCount || containers.length) - multiWriterSensitiveStores.length,
			violations: multiWriterSensitiveStores.map((c) => c.data.name),
		},
	];

	const passingFitnessRules = fitnessChecks.filter((c) => c.violations.length === 0).length;
	const fitnessScorePercent = Math.round((passingFitnessRules / fitnessChecks.length) * 100);

	// 2. Message & Contract Distribution Metrics
	const totalMessagesCount = allMessages.length;
	const messageBreakdown: MessageBreakdownItem[] = [
		{
			label: "Integration Events",
			shortLabel: "Events",
			count: events.length,
			percent: Math.round((events.length / Math.max(totalMessagesCount, 1)) * 100),
			colorClass: "bg-amber-500",
			borderClass: "border-amber-500/30",
			textClass: "text-amber-500",
			href: `${base}/discover/events`,
			description: "Asynchronous domain & integration outcomes published over Kafka broker",
		},
		{
			label: "Commands",
			shortLabel: "Commands",
			count: commands.length,
			percent: Math.round((commands.length / Math.max(totalMessagesCount, 1)) * 100),
			colorClass: "bg-sky-500",
			borderClass: "border-sky-500/30",
			textClass: "text-sky-500",
			href: `${base}/discover/commands`,
			description: "State-mutating HTTP, gRPC, and Kafka saga commands",
		},
		{
			label: "Queries",
			shortLabel: "Queries",
			count: queries.length,
			percent: Math.round((queries.length / Math.max(totalMessagesCount, 1)) * 100),
			colorClass: "bg-emerald-500",
			borderClass: "border-emerald-500/30",
			textClass: "text-emerald-500",
			href: `${base}/discover/queries`,
			description: "Read-only HTTP & gRPC synchronous data retrieval contracts",
		},
	];

	// 3. Test Verification Pyramid Metrics
	const unitTestProjectsCount = 9;
	const archTestProjectsCount = 1;
	const pactConsumerSuitesCount = 6;
	const pactProviderSuitesCount = 6;
	const frontendSuitesCount = 2;
	const totalAutomatedSuites =
		unitTestProjectsCount +
		archTestProjectsCount +
		pactConsumerSuitesCount +
		pactProviderSuitesCount +
		frontendSuitesCount;

	const verificationLayers: VerificationLayer[] = [
		{
			tier: "L1 · Unit & Domain Logic",
			count: unitTestProjectsCount + 2,
			unit: "11 suites (9 .NET TUnit + 2 Vitest)",
			tools: "TUnit · Vitest · MTP Coverage",
			workflow: "sonar.yml & frontend-ci.yml",
			widthPercent: 100,
			barColor: "bg-emerald-500",
		},
		{
			tier: "L2 · Consumer & Provider Contracts",
			count: pactConsumerSuitesCount + pactProviderSuitesCount,
			unit: "12 Pact suites (6 Consumer · 6 Provider)",
			tools: "Pact.NET · AsyncAPI · OpenAPI · Protobuf",
			workflow: "sonar.yml",
			widthPercent: 82,
			barColor: "bg-sky-500",
		},
		{
			tier: "L3 · Architecture & Fitness Invariants",
			count: archTestProjectsCount + fitnessChecks.length,
			unit: `1 ArchTest assembly + ${fitnessChecks.length} Catalog Fitness rules`,
			tools: "TUnit ArchTests · @eventcatalog/fitness · Turbo Boundaries",
			workflow: "sonar.yml & docs.yml",
			widthPercent: 64,
			barColor: "bg-indigo-500",
		},
		{
			tier: "L4 · End-to-End & Accessibility Journeys",
			count: frontendSuitesCount,
			unit: "2 E2E apps (Storefront & Backoffice)",
			tools: "Playwright BDD · axe-core a11y · Allure",
			workflow: "frontend-ci.yml",
			widthPercent: 46,
			barColor: "bg-amber-500",
		},
	];

	// 4. Team Ownership Distribution
	const teamOwnershipStats: TeamOwnershipStat[] = teams.map((team) => {
		const teamId = team.data.id;
		const ownedServices = services.filter((s) =>
			(s.data.owners ?? []).map(extractOwnerId).includes(teamId)
		);
		const ownedMessages = allMessages.filter(({ entry }) =>
			(entry.data.owners ?? []).map(extractOwnerId).includes(teamId)
		);
		const ownedDomains = domains.filter((d) =>
			(d.data.owners ?? []).map(extractOwnerId).includes(teamId)
		);
		const ownedContainers = containers.filter((c) =>
			(c.data.owners ?? []).map(extractOwnerId).includes(teamId)
		);
		return {
			id: teamId,
			name: team.data.name,
			summary: team.data.summary,
			servicesCount: ownedServices.length,
			messagesCount: ownedMessages.length,
			domainsCount: ownedDomains.length,
			containersCount: ownedContainers.length,
			totalAssets:
				ownedServices.length + ownedMessages.length + ownedDomains.length + ownedContainers.length,
		};
	});
	const maxTeamAssets = Math.max(...teamOwnershipStats.map((t) => t.totalAssets), 1);

	// 5. Enriched Service Quality & Contract Coverage Rows
	const enrichedServices: EnrichedService[] = services
		.map((service) => {
			const id = service.data.id;
			const isExternal = isExternalService(service);
			const specs = getServiceSpecLabels(service);
			const hasInlineSpec = INLINE_DOC_SERVICES.has(id);
			const sendsCount = (service.data.sends ?? []).length;
			const receivesCount = (service.data.receives ?? []).length;
			const readsCount = (service.data.readsFrom ?? []).length;
			const writesCount = (service.data.writesTo ?? []).length;
			const owners = (service.data.owners ?? []).map((o) => {
				const ownerId = extractOwnerId(o);
				return {
					id: ownerId,
					name: teamMap.get(ownerId) ?? userMap.get(ownerId) ?? ownerId,
				};
			});

			return {
				id,
				name: service.data.name,
				version: service.data.version,
				summary: service.data.summary,
				isExternal,
				specs: specs.length > 0 ? specs : hasInlineSpec ? ["INLINE / MCP"] : [],
				sendsCount,
				receivesCount,
				totalMessages: sendsCount + receivesCount,
				storesCount: new Set([
					...(service.data.readsFrom ?? []).map((r) => r.id),
					...(service.data.writesTo ?? []).map((w) => w.id),
				]).size,
				readsCount,
				writesCount,
				owners,
				testMapping: SERVICE_TEST_MATRIX[id],
				language: service.data.repository?.language ?? (isExternal ? "External" : "C# / .NET"),
			};
		})
		.sort((a, b) => {
			if (a.isExternal !== b.isExternal) return a.isExternal ? 1 : -1;
			return b.totalMessages - a.totalMessages;
		});

	const maxServiceMessages = Math.max(...enrichedServices.map((s) => s.totalMessages), 1);

	return {
		counts: {
			domains: domains.length,
			systems: systems.length,
			services: services.length,
			internalServices: internalServicesCount,
			containers: containers.length,
			agents: agents.length,
			entities: entities.length,
			flows: flows.length,
			channels: channels.length,
			adrs: adrs.length,
			teams: teams.length,
			users: users.length,
			events: events.length,
			commands: commands.length,
			queries: queries.length,
			totalMessages: totalMessagesCount,
		},
		firstTeamId: teams[0]?.data.id ?? "platform-team",
		fitnessChecks,
		passingFitnessRules,
		fitnessScorePercent,
		messageBreakdown,
		totalAutomatedSuites,
		verificationLayers,
		teamOwnershipStats,
		maxTeamAssets,
		enrichedServices,
		maxServiceMessages,
	};
}
