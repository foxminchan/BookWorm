import http from "k6/http";
import { getBaseUrl, testBookDetails } from "../utils/helpers";
import type { SeededRandom } from "../utils/seeded-random";
import type { TestDataGenerator } from "../utils/test-data";
import {
	testEndpointWithRetry,
	validatePagedResponse,
	validateResponse,
} from "../utils/validation";

export function browseCatalogScenario(
	dataGen: TestDataGenerator,
	random: SeededRandom,
): void {
	try {
		// Test basic catalog browsing - simulates users browsing the bookstore
		const booksResponse = http.get(`${getBaseUrl()}/catalog/api/v1/books`, {
			tags: { scenario: "browse_catalog", endpoint: "books" },
			timeout: "10s",
		});
		const booksData = validateResponse(booksResponse, "books", 200, 1200) as {
			items?: Array<{ id?: string }>;
		} | null;
		validatePagedResponse(booksData, "books");

		// Test categories endpoint
		const categoriesResponse = http.get(
			`${getBaseUrl()}/catalog/api/v1/categories`,
			{
				tags: { scenario: "browse_catalog", endpoint: "categories" },
				timeout: "10s",
			},
		);
		validateResponse(categoriesResponse, "categories", 200, 800);

		// Simulate user viewing a specific category if available (70% chance)
		if (random.bool(0.7)) {
			const categoryParams = {
				category: dataGen.getRandomCategory(),
				pageSize: dataGen.getRandomPageSize(),
				pageIndex: 1,
			};
			const categoryBooksResponse = testEndpointWithRetry(
				`${getBaseUrl()}/catalog/api/v1/books`,
				categoryParams,
				"category_books",
			);
			if ("body" in categoryBooksResponse) {
				validateResponse(categoryBooksResponse, "category_books");
			}
		}

		// Simulate browsing authors (50% chance)
		if (random.bool(0.5)) {
			const authorsResponse = http.get(
				`${getBaseUrl()}/catalog/api/v1/authors`,
				{
					tags: { scenario: "browse_catalog", endpoint: "authors" },
					timeout: "10s",
				},
			);
			validateResponse(authorsResponse, "authors", 200, 800);
		}

		// Simulate viewing a specific book (30% chance)
		if (
			random.bool(0.3) &&
			booksData?.items &&
			Array.isArray(booksData.items) &&
			booksData.items.length > 0
		) {
			const randomBook =
				booksData.items[random.int(0, booksData.items.length - 1)];
			if (randomBook?.id) {
				testBookDetails(randomBook.id, "browse_book_details");
			}
		}

		// Simulate pagination browsing (40% chance)
		if (random.bool(0.4)) {
			const paginationParams = {
				pageIndex: dataGen.getRandomPageIndex(),
				pageSize: dataGen.getRandomPageSize(),
			};
			const queryString = Object.entries(paginationParams)
				.map(
					([key, value]) =>
						`${encodeURIComponent(key)}=${encodeURIComponent(String(value))}`,
				)
				.join("&");
			const requestUrl = `${getBaseUrl()}/catalog/api/v1/books?${queryString}`;
			const paginationResponse = http.get(requestUrl, {
				tags: { scenario: "browse_catalog", endpoint: "books_pagination" },
				timeout: "10s",
			});
			validateResponse(paginationResponse, "pagination_browse");
		}
	} catch (error) {
		const errorMsg = error instanceof Error ? error.message : "Unknown error";
		console.error(`Error in browseCatalogScenario: ${errorMsg}`);
	}
}
