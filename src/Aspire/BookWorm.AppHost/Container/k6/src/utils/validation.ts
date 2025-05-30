import http from "k6/http";
import { sleep } from "k6";
import { CONSTANTS } from "../config";

/**
 * Enhanced helper functions for better test organization and debugging
 */
export function validateResponse(
  response: any,
  name: string,
  expectedStatus: number = CONSTANTS.HTTP_OK,
  _maxDuration: number = CONSTANTS.RESPONSE_TIME_THRESHOLD_95
): any {
  // Only perform detailed validation for successful responses
  if (
    response.status === expectedStatus &&
    expectedStatus === CONSTANTS.HTTP_OK
  ) {
    try {
      // Validate JSON structure for successful responses
      if (
        response.headers["Content-Type"]?.includes("application/json") &&
        response.body
      ) {
        const data = JSON.parse(response.body);
        if (data && typeof data === "object") {
          return data;
        }
      }
    } catch (error) {
      const errorMsg = error instanceof Error ? error.message : "Unknown error";
      console.warn(`JSON parsing failed for ${name}:`, errorMsg);
      console.warn(
        `Response status: ${response.status}, Body: ${
          response.body?.substring(0, 200) ?? "empty"
        }...`
      );
    }
  }

  // For non-OK responses, just log the status
  if (response.status !== expectedStatus) {
    console.warn(
      `${name} returned unexpected status: ${response.status} (expected: ${expectedStatus})`
    );
  }

  return null;
}

export function validatePagedResponse(data: any, name: string): boolean {
  if (!data || typeof data !== "object") {
    console.warn(`${name} - No valid data to validate pagination`);
    return false;
  }

  try {
    // Check for basic paged response structure - using correct property names
    const hasItems = data.hasOwnProperty("items") && Array.isArray(data.items);
    const hasPageIndex =
      data.hasOwnProperty("pageIndex") && typeof data.pageIndex === "number";
    const hasPageSize =
      data.hasOwnProperty("pageSize") && typeof data.pageSize === "number";
    const hasTotalItems =
      data.hasOwnProperty("totalItems") && typeof data.totalItems === "number";

    if (!hasItems) {
      console.warn(`${name} - Missing or invalid 'items' array`);
      return false;
    }

    if (!hasPageIndex || data.pageIndex < 0) {
      console.warn(`${name} - Invalid pageIndex: ${data.pageIndex}`);
      return false;
    }

    if (!hasPageSize || data.pageSize <= 0) {
      console.warn(`${name} - Invalid pageSize: ${data.pageSize}`);
      return false;
    }

    if (!hasTotalItems || data.totalItems < 0) {
      console.warn(`${name} - Invalid totalItems: ${data.totalItems}`);
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
function buildUrlWithParams(url: string, params: any): string {
  if (!params || Object.keys(params).length === 0) {
    return url;
  }

  const queryString = Object.entries(params)
    .map(
      ([key, value]) =>
        `${encodeURIComponent(key)}=${encodeURIComponent(String(value))}`
    )
    .join("&");
  return `${url}${url.includes("?") ? "&" : "?"}${queryString}`;
}

export function testEndpointWithRetry(
  url: string,
  params: any,
  name: string,
  maxRetries: number = 2
): any {
  let response: any;
  let retries = 0;
  const requestUrl = buildUrlWithParams(url, params);

  do {
    try {
      response = http.get(requestUrl, {
        tags: { name, retry: retries.toString() },
        timeout: "10s",
      } as any);
      if (response.status === CONSTANTS.HTTP_OK || retries >= maxRetries) break;
    } catch (error) {
      const errorMsg = error instanceof Error ? error.message : "Unknown error";
      console.warn(
        `Request failed for ${name} (attempt ${retries + 1}): ${errorMsg}`
      );
      response = { status: 0, timings: { duration: 10000 } }; // Mock failed response
    }

    retries++;
    if (retries <= maxRetries) {
      console.warn(
        `Retrying ${name} (attempt ${retries + 1}/${maxRetries + 1})`
      );
      sleep(0.5 * retries); // Exponential backoff
    }
  } while (retries <= maxRetries);

  if (response.status !== CONSTANTS.HTTP_OK && retries > 0) {
    console.warn(
      `${name} failed after ${retries} retries. Final status: ${response.status}`
    );
  }

  return response;
}
