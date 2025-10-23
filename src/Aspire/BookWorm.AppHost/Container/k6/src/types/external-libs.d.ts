// TypeScript declarations for external K6 libraries loaded via URLs

declare module "https://jslib.k6.io/k6-summary/0.0.1/index.js" {
	/**
	 * Generates a text summary of K6 test results
	 * @param data The test results data from K6
	 * @param options Configuration options for the summary
	 * @returns Formatted text summary string
	 */
	export function textSummary(
		data: Record<string, unknown>,
		options?: {
			indent?: string;
			enableColors?: boolean;
		},
	): string;
}

declare module "https://raw.githubusercontent.com/benc-uk/k6-reporter/main/dist/bundle.js" {
	/**
	 * Generates an HTML report of K6 test results
	 * @param data The test results data from K6
	 * @param options Configuration options for the HTML report
	 * @returns HTML report string
	 */
	export function htmlReport(
		data: Record<string, unknown>,
		options?: {
			title?: string;
			logoUrl?: string;
		},
	): string;
}
