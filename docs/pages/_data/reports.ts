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
