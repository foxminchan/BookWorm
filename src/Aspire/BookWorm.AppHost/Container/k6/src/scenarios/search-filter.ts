import http from "k6/http";
import { getBaseUrl } from "../utils/helpers";
import type { SeededRandom } from "../utils/seeded-random";
import type { TestDataGenerator } from "../utils/test-data";
import {
	testEndpointWithRetry,
	validatePagedResponse,
	validateResponse,
} from "../utils/validation";

export function searchFilterScenario(dataGen: TestDataGenerator, random: SeededRandom): void {
	try {
		// Test comprehensive search and filtering functionality
		const searchTerm = dataGen.getRandomSearchTerm();
		const minPrice = dataGen.getRandomPrice(5, 25);
		const maxPrice = dataGen.getRandomPrice(26, 100);
		const sortOption = dataGen.getRandomSortOption();

		// Test search with various parameters
		const searchParams = {
			search: searchTerm,
			minPrice,
			maxPrice,
			pageIndex: 1,
			pageSize: dataGen.getRandomPageSize(),
			...sortOption,
		};

		const searchResponse = testEndpointWithRetry(
			`${getBaseUrl()}/catalog/api/v1/books`,
			searchParams,
			"search"
		);
		if ("body" in searchResponse) {
			const searchData = validateResponse(searchResponse, "search", 200, 800);
			validatePagedResponse(searchData, "search");
		}

		// Test price-only filtering
		const priceFilterParams = {
			minPrice: dataGen.getRandomPrice(10, 30),
			maxPrice: dataGen.getRandomPrice(31, 80),
			pageIndex: 1,
			pageSize: 10,
		};

		const queryString = Object.entries(priceFilterParams)
			.map(([key, value]) => `${encodeURIComponent(key)}=${encodeURIComponent(String(value))}`)
			.join("&");
		const requestUrl = `${getBaseUrl()}/catalog/api/v1/books?${queryString}`;
		const priceFilterResponse = http.get(requestUrl, {
			tags: { scenario: "search_filter", endpoint: "price_filter" },
		});
		validateResponse(priceFilterResponse, "price_filter");

		// Test pagination with different page sizes
		if (random.bool(0.5)) {
			const paginationParams = {
				pageIndex: random.int(1, 3), // Pages 1-3
				pageSize: dataGen.getRandomPageSize(),
			};

			const queryString = Object.entries(paginationParams)
				.map(([key, value]) => `${encodeURIComponent(key)}=${encodeURIComponent(String(value))}`)
				.join("&");
			const requestUrl = `${getBaseUrl()}/catalog/api/v1/books?${queryString}`;
			const paginationResponse = http.get(requestUrl, {
				tags: { scenario: "search_filter", endpoint: "pagination" },
			});
			validateResponse(paginationResponse, "pagination");
		}
	} catch (error) {
		const errorMsg = error instanceof Error ? error.message : "Unknown error";
		console.error(`Error in searchFilterScenario: ${errorMsg}`);
	}
}
