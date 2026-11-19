export type AllureReportSuite = {
	id: string;
	name: string;
	category: string;
	badge: string;
	description: string;
	reportUrl: string;
	workflow: string;
	artifact: string;
};

export type AllureResultFeed = {
	id: string;
	name: string;
	scope: string;
	artifact: string;
	workflow: string;
};

export type ServiceTestMapping = {
	serviceId: string;
	unitTestProject?: string;
	integrationTestProject?: string;
	contractConsumerProject?: string;
	contractProviderProject?: string;
	e2eSuite?: string;
};

export type RoleGuide = {
	id: "director" | "pm" | "qa" | "dev";
	role: string;
	title: string;
	summary: string;
	keyQuestions: string[];
	focusSignals: { label: string; target: string }[];
};

export const REPORT_SUITES: AllureReportSuite[] = [
	{
		id: "unified",
		name: "Unified Test Report",
		category: "All Suites",
		badge: "Unified",
		description:
			"Combined Allure report merging Backend Unit, Backend Contract, Backoffice, and Storefront test results.",
		reportUrl: "/reports/index.html",
		workflow: "docs.yml",
		artifact: "Merged via allure generate",
	},
	{
		id: "backend",
		name: "Backend Report",
		category: "Backend",
		badge: "BE",
		description:
			"Unit, architecture, integration, and contract test execution report for BookWorm .NET services.",
		reportUrl: "/reports/backend/index.html",
		workflow: "sonar.yml",
		artifact: "allure-report-backend",
	},
	{
		id: "backoffice",
		name: "Backoffice Report",
		category: "Frontend",
		badge: "FE",
		description:
			"Unit and E2E test execution report for the BookWorm Backoffice administrative portal.",
		reportUrl: "/reports/backoffice/index.html",
		workflow: "frontend-ci.yml",
		artifact: "allure-report-backoffice",
	},
	{
		id: "storefront",
		name: "Storefront Report",
		category: "Frontend",
		badge: "FE",
		description:
			"Unit and E2E test execution report for the BookWorm customer-facing Storefront web app.",
		reportUrl: "/reports/storefront/index.html",
		workflow: "frontend-ci.yml",
		artifact: "allure-report-storefront",
	},
];

export const RESULT_FEEDS: AllureResultFeed[] = [
	{
		id: "be-unit",
		name: "Backend Unit Tests",
		scope: "Microservices & BuildingBlocks",
		artifact: "allure-results-be-unit",
		workflow: "sonar.yml",
	},
	{
		id: "be-contract",
		name: "Backend Contract Tests",
		scope: "Service & Messaging Contracts",
		artifact: "allure-results-be-contract",
		workflow: "sonar.yml",
	},
	{
		id: "fe-bo",
		name: "Backoffice Results",
		scope: "Next.js Unit & E2E",
		artifact: "allure-results-fe-bo",
		workflow: "frontend-ci.yml",
	},
	{
		id: "fe-sf",
		name: "Storefront Results",
		scope: "Next.js Unit & E2E",
		artifact: "allure-results-fe-sf",
		workflow: "frontend-ci.yml",
	},
];

export const SERVICE_TEST_MATRIX: Record<string, Omit<ServiceTestMapping, "serviceId">> = {
	"basket-api": {
		unitTestProject: "BookWorm.Basket.UnitTests",
		contractConsumerProject: "BookWorm.Basket.ContractTests.PactConsumer",
		contractProviderProject: "BookWorm.Basket.ContractTests.PactProvider",
	},
	"catalog-api": {
		unitTestProject: "BookWorm.Catalog.UnitTests",
		contractConsumerProject: "BookWorm.Catalog.ContractTests.PactConsumer",
		contractProviderProject: "BookWorm.Catalog.ContractTests.PactProvider",
	},
	"catalog-ingestion": {
		unitTestProject: "BookWorm.Catalog.UnitTests",
	},
	"chat-api": {
		unitTestProject: "BookWorm.Chat.UnitTests",
	},
	"mcp-server": {
		unitTestProject: "BookWorm.McpTools.UnitTests",
	},
	"notification-service": {
		unitTestProject: "BookWorm.Notification.UnitTests",
		contractConsumerProject: "BookWorm.Notification.ContractTests.PactConsumer",
	},
	"order-api": {
		unitTestProject: "BookWorm.Ordering.UnitTests",
		contractConsumerProject: "BookWorm.Ordering.ContractTests.PactConsumer",
		contractProviderProject: "BookWorm.Ordering.ContractTests.PactProvider",
	},
	"order-orchestration": {
		unitTestProject: "BookWorm.Finance.UnitTests",
		contractConsumerProject: "BookWorm.Finance.ContractTests.PactConsumer",
		contractProviderProject: "BookWorm.Finance.ContractTests.PactProvider",
	},
	"rating-api": {
		unitTestProject: "BookWorm.Rating.UnitTests",
		contractConsumerProject: "BookWorm.Rating.ContractTests.PactConsumer",
		contractProviderProject: "BookWorm.Rating.ContractTests.PactProvider",
	},
	"scheduler-service": {
		unitTestProject: "BookWorm.Scheduler.UnitTests",
		contractProviderProject: "BookWorm.Scheduler.ContractTests.PactProvider",
	},
};

export const ROLE_GUIDES: RoleGuide[] = [
	{
		id: "director",
		role: "Director / Engineering Leadership",
		title: "Executive Governance & Risk Posture",
		summary:
			"Platform-wide governance scorecards, team ownership coverage, architectural compliance, and security baseline across all bounded contexts.",
		keyQuestions: [
			"Are all internal services and message contracts accountable to a defined team?",
			"Do we have single-writer boundaries on confidential and regulated data stores?",
			"What is our overall architectural fitness and security scorecard posture?",
		],
		focusSignals: [
			{ label: "Architecture Fitness Rules", target: "#section-fitness" },
			{ label: "Team Ownership Distribution", target: "#section-charts" },
			{ label: "SonarCloud & OpenSSF Scorecard", target: "#section-kpis" },
		],
	},
	{
		id: "pm",
		role: "Product Manager",
		title: "Domain Capability & Journey Readiness",
		summary:
			"Business domain inventory, end-to-end customer journey coverage, and service health across Catalog, Shopping, Ordering, Reviews, and Conversational AI.",
		keyQuestions: [
			"Which business capabilities, user flows, and domain events are live and documented?",
			"Are customer-facing journeys (Storefront & Backoffice) backed by verified E2E suites?",
			"Which team owns each product capability and service boundary?",
		],
		focusSignals: [
			{ label: "Domain & System Footprint", target: "#section-charts" },
			{ label: "Storefront & Backoffice Suites", target: "#section-suites" },
			{ label: "Service & Flow Coverage Matrix", target: "#section-matrix" },
		],
	},
	{
		id: "qa",
		role: "Quality Assurance",
		title: "Test Verification & Contract Matrix",
		summary:
			"Multi-tier test verification pyramid spanning Unit, Architecture, Pact Consumer/Provider Contracts, and Playwright BDD E2E suites.",
		keyQuestions: [
			"Are all microservices covered by unit tests and bidirectional Pact message contracts?",
			"Do internal services publish formal OpenAPI, AsyncAPI, or Protobuf specifications?",
			"Where can I inspect the latest Allure and TUnit execution reports per test suite?",
		],
		focusSignals: [
			{ label: "Test Verification Pyramid", target: "#section-charts" },
			{ label: "Service Test & Contract Matrix", target: "#section-matrix" },
			{ label: "Allure & TUnit Report Explorers", target: "#section-suites" },
		],
	},
	{
		id: "dev",
		role: "Developer / Architect",
		title: "Contract Specs, Fitness Checks & CI Diagnostics",
		summary:
			"Actionable violation diagnostics, circular command dependency detection, orphaned event tracking, and per-project TUnit test reports.",
		keyQuestions: [
			"Are any architecture fitness functions or contract invariants failing right now?",
			"Do all latest events have documented producers and valid schema specifications?",
			"Which CI workflow and artifact produces each test feed and TUnit report?",
		],
		focusSignals: [
			{ label: "Fitness Function Checks", target: "#section-fitness" },
			{ label: "Service Specifications & Contracts", target: "#section-matrix" },
			{ label: "TUnit Per-Project Reports", target: "#section-suites" },
		],
	},
];
