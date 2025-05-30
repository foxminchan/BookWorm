import http from "k6/http";
import { TestDataGenerator } from "../utils/test-data";
import { validateResponse } from "../utils/validation";
import { getBaseUrl } from "../utils/helpers";
import { SeededRandom } from "../utils/seeded-random";
import { CONSTANTS } from "../config";

export function stressTestScenario(
  dataGen: TestDataGenerator,
  random: SeededRandom
): void {
  try {
    // High-intensity testing with concurrent requests
    const requests = [
      { url: `${getBaseUrl()}/catalog/api/v1/books`, name: "books_stress" },
      {
        url: `${getBaseUrl()}/catalog/api/v1/categories`,
        name: "categories_stress",
      },
      { url: `${getBaseUrl()}/catalog/api/v1/authors`, name: "authors_stress" },
      {
        url: `${getBaseUrl()}/catalog/api/v1/publishers`,
        name: "publishers_stress",
      },
    ];

    // Make multiple concurrent requests
    const responses = requests.map((req) => http.get(req.url));

    // Validate all responses
    responses.forEach((response, index) => {
      validateResponse(response, requests[index].name, CONSTANTS.HTTP_OK, 1000); // More lenient timing for stress test
    });

    // Additional complex search under stress
    const complexSearchParams = {
      search: dataGen.getRandomSearchTerm(),
      minPrice: dataGen.getRandomPrice(5, 50),
      maxPrice: dataGen.getRandomPrice(51, 100),
      pageIndex: random.int(1, 5),
      pageSize: dataGen.getRandomPageSize(),
      orderBy: dataGen.getRandomSortOption().orderBy,
      isDescending: random.bool(0.5),
    };

    const queryString = Object.entries(complexSearchParams)
      .map(
        ([key, value]) =>
          `${encodeURIComponent(key)}=${encodeURIComponent(String(value))}`
      )
      .join("&");
    const requestUrl = `${getBaseUrl()}/catalog/api/v1/books?${queryString}`;
    const stressSearchResponse = http.get(requestUrl, {
      tags: { scenario: "stress_test", endpoint: "stress_search" },
    } as any);
    validateResponse(
      stressSearchResponse,
      "stress_search",
      CONSTANTS.HTTP_OK,
      1200
    );
  } catch (error) {
    const errorMsg = error instanceof Error ? error.message : "Unknown error";
    console.error(`Error in stressTestScenario: ${errorMsg}`);
  }
}
