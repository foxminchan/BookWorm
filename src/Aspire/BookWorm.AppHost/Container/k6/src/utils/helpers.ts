import { check } from "k6";
import http from "k6/http";
import { CONSTANTS } from "../config";

/**
 * Test specific book details
 */
export function testBookDetails(
	bookId: string,
	name: string = "book_details",
): unknown {
	const response = http.get(`${getBaseUrl()}/catalog/api/v1/books/${bookId}`);
	const data = validateResponse(response, name) as Record<
		string,
		unknown
	> | null;

	if (data && response.status === CONSTANTS.HTTP_OK) {
		check(response, {
			[`${name} should have id`]: () => Object.hasOwn(data, "id"),
			[`${name} should have title`]: () => Object.hasOwn(data, "title"),
			[`${name} should have price`]: () => Object.hasOwn(data, "price"),
			[`${name} price should be positive`]: () =>
				typeof data.price === "number" && data.price > 0,
		});
	}

	return data;
}

/**
 * Basic connectivity check
 */
export function checkServiceAvailability(): boolean {
	const response = http.get(`${getBaseUrl()}/catalog/health`, {
		timeout: "5s",
	});

	if (response.status === 0) {
		console.error(
			`Service not available at ${getBaseUrl()}. Check if the service is running.`,
		);
		return false;
	}

	// Accept various health check status codes
	const acceptableStatuses = [CONSTANTS.HTTP_OK, 404]; // Some services might not have /health endpoint
	if (!acceptableStatuses.includes(response.status)) {
		console.warn(
			`Service at ${getBaseUrl()} responded with status ${response.status}`,
		);
	}

	return true;
}

/**
 * Get base URL from environment
 */
export function getBaseUrl(): string {
	const baseUrl = __ENV.services__gateway__http__0;
	if (!baseUrl) {
		throw new Error(
			"BASE_URL is not set. Please provide services__gateway__http__0 environment variable.",
		);
	}
	return baseUrl;
}

/**
 * Simple response validation
 */
function validateResponse(response: http.Response, name: string): unknown {
	if (response.status === CONSTANTS.HTTP_OK) {
		try {
			if (
				response.headers["Content-Type"]?.includes("application/json") &&
				response.body &&
				typeof response.body === "string"
			) {
				return JSON.parse(response.body);
			}
		} catch (error) {
			const errorMsg = error instanceof Error ? error.message : "Unknown error";
			console.warn(`JSON parsing failed for ${name}:`, errorMsg);
		}
	}
	return null;
}
