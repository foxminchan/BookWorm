import { sleep } from "k6";
import http from "k6/http";
import { CONSTANTS } from "../config";

/**
 * Parse JSON response body safely
 */
function parseJsonBody(body: string | ArrayBuffer | null): unknown | null {
	if (typeof body !== "string") {
		return null;
	}

	try {
		const data = JSON.parse(body);
		return data && typeof data === "object" ? data : null;
	} catch {
		return null;
	}
}

/**
 * Log JSON parsing error with context
 */
function logParsingError(
	name: string,
	error: unknown,
	response: http.Response,
): void {
	const errorMsg = error instanceof Error ? error.message : "Unknown error";
	console.warn(`JSON parsing failed for ${name}:`, errorMsg);

	const bodyPreview =
		typeof response.body === "string"
			? response.body.substring(0, 200)
			: "binary data";
	console.warn(`Response status: ${response.status}, Body: ${bodyPreview}...`);
}

/**
 * Check if response has JSON content type
 */
function isJsonResponse(response: http.Response): boolean {
	return (
		!!response.headers["Content-Type"]?.includes("application/json") &&
		!!response.body
	);
}

/**
 * Enhanced helper functions for better test organization and debugging
 */
export function validateResponse(
	response: http.Response,
	name: string,
	expectedStatus: number = CONSTANTS.HTTP_OK,
	_maxDuration: number = CONSTANTS.RESPONSE_TIME_THRESHOLD_95,
): unknown {
	// Log unexpected status
	if (response.status !== expectedStatus) {
		console.warn(
			`${name} returned unexpected status: ${response.status} (expected: ${expectedStatus})`,
		);
		return null;
	}

	// Only parse JSON for successful responses
	if (expectedStatus !== CONSTANTS.HTTP_OK || !isJsonResponse(response)) {
		return null;
	}

	const data = parseJsonBody(response.body);
	if (data) {
		return data;
	}

	// Log parsing error
	logParsingError(name, new Error("JSON parsing failed"), response);
	return null;
}

export function validatePagedResponse(data: unknown, name: string): boolean {
	if (!data || typeof data !== "object") {
		console.warn(`${name} - No valid data to validate pagination`);
		return false;
	}

	try {
		const record = data as Record<string, unknown>;
		// Check for basic paged response structure - using correct property names
		const hasItems =
			Object.hasOwn(record, "items") && Array.isArray(record.items);
		const hasPageIndex =
			Object.hasOwn(record, "pageIndex") &&
			typeof record.pageIndex === "number";
		const hasPageSize =
			Object.hasOwn(record, "pageSize") && typeof record.pageSize === "number";
		const hasTotalItems =
			Object.hasOwn(record, "totalItems") &&
			typeof record.totalItems === "number";

		if (!hasItems) {
			console.warn(`${name} - Missing or invalid 'items' array`);
			return false;
		}

		if (!hasPageIndex || (record.pageIndex as number) < 0) {
			console.warn(`${name} - Invalid pageIndex: ${record.pageIndex}`);
			return false;
		}

		if (!hasPageSize || (record.pageSize as number) <= 0) {
			console.warn(`${name} - Invalid pageSize: ${record.pageSize}`);
			return false;
		}

		if (!hasTotalItems || (record.totalItems as number) < 0) {
			console.warn(`${name} - Invalid totalItems: ${record.totalItems}`);
			return false;
		}

		return true;
	} catch (error) {
		const errorMsg = error instanceof Error ? error.message : "Unknown error";
		console.error(`Paged response validation failed for ${name}:`, errorMsg);
		return false;
	}
}

/**
 * Build URL with query parameters
 */
function buildUrlWithParams(
	url: string,
	params: Record<string, unknown>,
): string {
	if (!params || Object.keys(params).length === 0) {
		return url;
	}

	const queryString = Object.entries(params)
		.map(
			([key, value]) =>
				`${encodeURIComponent(key)}=${encodeURIComponent(String(value))}`,
		)
		.join("&");
	return `${url}${url.includes("?") ? "&" : "?"}${queryString}`;
}

export function testEndpointWithRetry(
	url: string,
	params: Record<string, unknown>,
	name: string,
	maxRetries: number = 2,
): http.Response | { status: number; timings: { duration: number } } {
	let response:
		| http.Response
		| { status: number; timings: { duration: number } };
	let retries = 0;
	const requestUrl = buildUrlWithParams(url, params);

	do {
		try {
			response = http.get(requestUrl, {
				tags: { name, retry: retries.toString() },
				timeout: "10s",
			});
			if (response.status === CONSTANTS.HTTP_OK || retries >= maxRetries) break;
		} catch (error) {
			const errorMsg = error instanceof Error ? error.message : "Unknown error";
			console.warn(
				`Request failed for ${name} (attempt ${retries + 1}): ${errorMsg}`,
			);
			response = { status: 0, timings: { duration: 10000 } }; // Mock failed response
		}

		retries++;
		if (retries <= maxRetries) {
			console.warn(
				`Retrying ${name} (attempt ${retries + 1}/${maxRetries + 1})`,
			);
			sleep(0.5 * retries); // Exponential backoff
		}
	} while (retries <= maxRetries);

	if (response.status !== CONSTANTS.HTTP_OK && retries > 0) {
		console.warn(
			`${name} failed after ${retries} retries. Final status: ${response.status}`,
		);
	}

	return response;
}
