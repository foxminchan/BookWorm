export type AllureReportSuite = {
	id: string;
	name: string;
	category: string;
	badge: string;
	description: string;
	reportUrl: string;
	workflow: string;
	artifact: string;
	environments: string[];
};

export const DEFAULT_REPORT_SUITE_ID = "unified";

export const REPORT_SUITES: AllureReportSuite[] = [
	{
		id: "unified",
		name: "Unified Test Report",
		category: "All Suites",
		badge: "Unified",
		description:
			"Combined Allure report merging Backend Unit, Backend Architecture, Backend Contract, Backoffice, and Storefront test results.",
		reportUrl: "/reports/index.html",
		workflow: "docs.yml",
		artifact: "Merged via allure generate",
		environments: [
			"Backend Unit",
			"Backend Architecture",
			"Backend Contract & Integration",
			"Backoffice (FE)",
			"Storefront (FE)",
		],
	},
	{
		id: "backend",
		name: "Backend Report",
		category: "Backend",
		badge: "BE",
		description:
			"Unit, architecture fitness, integration, and contract test execution report for BookWorm .NET services.",
		reportUrl: "/reports/backend/index.html",
		workflow: "sonar.yml",
		artifact: "allure-report-backend",
		environments: [
			"Microservice Unit Tests",
			"Architecture Fitness Tests",
			"Contract & Integration Tests",
		],
	},
	{
		id: "backoffice",
		name: "Backoffice Report",
		category: "Frontend",
		badge: "FE",
		description:
			"Domain feature integration, UI component, hook/core utility, and E2E test execution report for the Backoffice administrative portal.",
		reportUrl: "/reports/backoffice/index.html",
		workflow: "frontend-ci.yml",
		artifact: "allure-report-backoffice",
		environments: [
			"Domain Features",
			"UI Components",
			"Hooks & Core Utilities",
			"E2E (Playwright BDD)",
		],
	},
	{
		id: "storefront",
		name: "Storefront Report",
		category: "Frontend",
		badge: "FE",
		description:
			"Domain feature integration, UI component, hook/atom, property-based fuzz, and E2E test execution report for the Storefront web app.",
		reportUrl: "/reports/storefront/index.html",
		workflow: "frontend-ci.yml",
		artifact: "allure-report-storefront",
		environments: [
			"Domain Features",
			"UI Components",
			"Hooks, Atoms & Utilities",
			"Property & Fuzz Tests",
			"E2E (Playwright BDD)",
		],
	},
];
