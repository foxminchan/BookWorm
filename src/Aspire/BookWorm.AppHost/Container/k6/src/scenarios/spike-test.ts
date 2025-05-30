import http from "k6/http";
import { TestDataGenerator } from "../utils/test-data";
import { validateResponse } from "../utils/validation";
import { getBaseUrl } from "../utils/helpers";
import { SeededRandom } from "../utils/seeded-random";
import { CONSTANTS } from "../config";

export function spikeTestScenario(
  dataGen: TestDataGenerator,
  random: SeededRandom
): void {
  try {
    // Simulate sudden traffic spikes - focuses on resilience
    const spikeEndpoints = [
      `${getBaseUrl()}/catalog/api/v1/books`,
      `${getBaseUrl()}/catalog/api/v1/categories`,
    ];

    const randomEndpoint =
      spikeEndpoints[random.int(0, spikeEndpoints.length - 1)];
    const response = http.get(randomEndpoint);

    // During spike tests, we're more forgiving with response times
    validateResponse(response, "spike_test", CONSTANTS.HTTP_OK, 1500);

    // Simulate rapid consecutive requests (like a user clicking rapidly)
    if (random.bool(0.3)) {
      // 30% chance
      const rapidResponse = http.get(randomEndpoint);
      validateResponse(rapidResponse, "rapid_request", CONSTANTS.HTTP_OK, 1500);
    }
  } catch (error) {
    const errorMsg = error instanceof Error ? error.message : "Unknown error";
    console.error(`Error in spikeTestScenario: ${errorMsg}`);
  }
}
