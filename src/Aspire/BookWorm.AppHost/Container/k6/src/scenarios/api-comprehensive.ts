import http from "k6/http";
import { check } from "k6";
import type { TestDataGenerator } from "../utils/test-data";
import { validateResponse } from "../utils/validation";
import { getBaseUrl } from "../utils/helpers";
import type { SeededRandom } from "../utils/seeded-random";
import { CONSTANTS } from "../config";

export function apiComprehensiveScenario(
	dataGen: TestDataGenerator,
	random: SeededRandom,
): void {
	try {
		// Test all available API endpoints systematically
		const endpoints = [
			{
				url: `${getBaseUrl()}/catalog/api/v1/books`,
				name: "books",
				maxDuration: 600,
			},
			{
				url: `${getBaseUrl()}/catalog/api/v1/categories`,
				name: "categories",
				maxDuration: 300,
			},
			{
				url: `${getBaseUrl()}/catalog/api/v1/authors`,
				name: "authors",
				maxDuration: 400,
			},
			{
				url: `${getBaseUrl()}/catalog/api/v1/publishers`,
				name: "publishers",
				maxDuration: 400,
			},
		];

		// Test all basic endpoints
		for (const endpoint of endpoints) {
			const response = http.get(endpoint.url, {
				tags: { scenario: "api_comprehensive", endpoint: endpoint.name },
			});
			validateResponse(
				response,
				endpoint.name,
				CONSTANTS.HTTP_OK,
				endpoint.maxDuration,
			);

			// Test with various query parameters for books endpoint
			if (endpoint.name === "books") {
				const paramCombinations = [
					{ pageSize: 5, orderBy: "Name", isDescending: false },
					{ pageSize: 10, orderBy: "Price", isDescending: true },
					{ minPrice: 10, maxPrice: 50 },
					{ category: dataGen.getRandomCategory() },
					{ search: dataGen.getRandomSearchTerm(), pageSize: 15 },
				];

				let index = 0;
				for (const params of paramCombinations) {
					// Build URL with query parameters
					const queryString = Object.entries(params)
						.map(
							([key, value]) =>
								`${encodeURIComponent(key)}=${encodeURIComponent(
									String(value),
								)}`,
						)
						.join("&");
					const requestUrl = `${endpoint.url}?${queryString}`;

					const paramsResponse = http.get(requestUrl, {
						tags: {
							scenario: "api_comprehensive",
							endpoint: `${endpoint.name}_params_${index}`,
						},
					} as any);
					validateResponse(
						paramsResponse,
						`${endpoint.name}_with_params_${index}`,
					);
					index++;
				}
			}
		}

		// Test edge cases and boundary conditions
		const edgeCases = [
			{ params: { pageSize: 1 }, name: "min_page_size" },
			{ params: { pageSize: 100 }, name: "max_page_size" },
			{ params: { pageIndex: 1000 }, name: "high_page_index" },
			{ params: { minPrice: 0, maxPrice: 1 }, name: "low_price_range" },
			{ params: { minPrice: 999, maxPrice: 1000 }, name: "high_price_range" },
		];

		for (const testCase of edgeCases) {
			// Build URL with query parameters
			const queryString = Object.entries(testCase.params)
				.map(
					([key, value]) =>
						`${encodeURIComponent(key)}=${encodeURIComponent(String(value))}`,
				)
				.join("&");
			const requestUrl = `${getBaseUrl()}/catalog/api/v1/books?${queryString}`;

			const response = http.get(requestUrl, {
				tags: {
					scenario: "api_comprehensive",
					endpoint: `edge_case_${testCase.name}`,
				},
			} as any);
			validateResponse(response, `edge_case_${testCase.name}`);
		}

		// Test error scenarios (20% chance)
		if (random.bool(0.2)) {
			const errorTests = [
				{
					url: `${getBaseUrl()}/catalog/api/v1/books/999999`,
					expectedStatus: CONSTANTS.HTTP_NOT_FOUND,
					name: "invalid_book_id",
				},
				{
					url: `${getBaseUrl()}/catalog/api/v1/categories/999999`,
					expectedStatus: CONSTANTS.HTTP_NOT_FOUND,
					name: "invalid_category_id",
				},
			];

			for (const test of errorTests) {
				const response = http.get(test.url, {
					tags: {
						scenario: "api_comprehensive",
						endpoint: `error_${test.name}`,
					},
				});
				validateResponse(response, test.name, test.expectedStatus, 1000);
			}
		}

		// Test malformed query parameters (10% chance)
		if (random.bool(0.1)) {
			const malformedParams = { pageSize: "invalid", minPrice: "not_a_number" };
			// Build URL with malformed query parameters
			const queryString = Object.entries(malformedParams)
				.map(
					([key, value]) =>
						`${encodeURIComponent(key)}=${encodeURIComponent(String(value))}`,
				)
				.join("&");
			const requestUrl = `${getBaseUrl()}/catalog/api/v1/books?${queryString}`;

			const malformedResponse = http.get(requestUrl, {
				tags: { scenario: "api_comprehensive", endpoint: "malformed_params" },
			} as any);

			// Expecting either 400 Bad Request or the API to handle gracefully
			const acceptableStatuses = [
				CONSTANTS.HTTP_OK,
				CONSTANTS.HTTP_BAD_REQUEST,
			];
			const isValidStatus = acceptableStatuses.includes(
				malformedResponse.status,
			);

			check(
				malformedResponse,
				{
					"malformed params handled appropriately": () => isValidStatus,
				},
				{ scenario: "api_comprehensive", endpoint: "malformed_params" },
			);
		}
	} catch (error) {
		const errorMsg = error instanceof Error ? error.message : "Unknown error";
		console.error(`Error in apiComprehensiveScenario: ${errorMsg}`);
	}
}
