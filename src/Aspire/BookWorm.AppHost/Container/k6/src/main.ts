import { textSummary } from "https://jslib.k6.io/k6-summary/0.0.1/index.js";
import { htmlReport } from "https://raw.githubusercontent.com/benc-uk/k6-reporter/main/dist/bundle.js";
import { sleep } from "k6";
import { apiComprehensiveScenario } from "./scenarios/api-comprehensive";
import { browseCatalogScenario } from "./scenarios/browse-catalog";
import { searchFilterScenario } from "./scenarios/search-filter";
import { spikeTestScenario } from "./scenarios/spike-test";
import { stressTestScenario } from "./scenarios/stress-test";
import type { K6SummaryData } from "./types";
import { checkServiceAvailability } from "./utils/helpers";
import { SeededRandom } from "./utils/seeded-random";
import { TestDataGenerator } from "./utils/test-data";

// Export the options for K6
export { options } from "./config";

// Initialize seeded random generator for test reproducibility
const testRandom = new SeededRandom(
	__ENV.RANDOM_SEED ? Number.parseInt(__ENV.RANDOM_SEED, 10) : 12345,
);
const dataGenerator = new TestDataGenerator(testRandom);

export const setup = () => {
	return {
		testStartTime: Date.now(),
	};
};

export default function main() {
	// Quick connectivity check
	if (!checkServiceAvailability()) {
		console.error(
			"Service availability check failed. Skipping test iteration.",
		);
		sleep(5); // Wait before retrying
		return;
	}

	const scenario = __ENV.scenario || "browse_catalog";

	switch (scenario) {
		case "browse_catalog":
			browseCatalogScenario(dataGenerator, testRandom);
			break;
		case "search_filter":
			searchFilterScenario(dataGenerator, testRandom);
			break;
		case "api_comprehensive":
			apiComprehensiveScenario(dataGenerator, testRandom);
			break;
		case "stress_test":
			stressTestScenario(dataGenerator, testRandom);
			break;
		case "spike_test":
			spikeTestScenario(dataGenerator, testRandom);
			break;
		default:
			browseCatalogScenario(dataGenerator, testRandom);
	}

	// Variable sleep time to simulate real user behavior
	sleep(testRandom.next() * 2 + 1); // 1-3 seconds
}

export function handleSummary(data: K6SummaryData) {
	// Generate comprehensive test summary
	const customSummary = {
		"summary.html": htmlReport(data),
		"summary.json": JSON.stringify(data, null, 2),
		stdout: textSummary(data, { indent: " ", enableColors: true }),
	};

	// Add custom summary with scenario breakdown
	if (data.metrics) {
		console.log("\n=== BookWorm K6 Performance Test Summary ===");
		console.log(`Test Duration: ${data.state.testRunDurationMs}ms`);
		console.log(
			`Total Requests: ${data.metrics.http_reqs?.values?.count ?? 0}`,
		);
		console.log(
			`Failed Requests: ${data.metrics.http_req_failed?.values?.fails ?? 0}`,
		);
		console.log(
			`Average Response Time: ${Math.round(
				data.metrics.http_req_duration?.values?.avg ?? 0,
			)}ms`,
		);
		console.log(
			`95th Percentile Response Time: ${Math.round(
				data.metrics.http_req_duration?.values?.["p(95)"] ?? 0,
			)}ms`,
		);
		const passes = data.metrics.checks?.values?.passes ?? 0;
		const fails = data.metrics.checks?.values?.fails ?? 0;
		const total = passes + fails;
		const successRate = total > 0 ? Math.round((passes / total) * 100) : 0;
		console.log(`Check Success Rate: ${successRate}%`);
		console.log("===============================================\n");
	}

	return customSummary;
}
