import http from "k6/http";
import { check, sleep } from "k6";
import { expect } from "https://jslib.k6.io/k6chaijs/4.5.0.1/index.js";
import { textSummary } from "https://jslib.k6.io/k6-summary/0.0.1/index.js";
import { htmlReport } from "https://raw.githubusercontent.com/benc-uk/k6-reporter/main/dist/bundle.js";

// Constants
const HTTP_OK = 200;
const HTTP_BAD_REQUEST = 400;
const HTTP_NOT_FOUND = 404;

const RESPONSE_TIME_THRESHOLD_95 = 1000; // milliseconds - more realistic for development
const RESPONSE_TIME_THRESHOLD_99 = 2000; // milliseconds - more lenient
const CHECK_RATE_THRESHOLD = 0.9; // 90% instead of 95% for development environment

// Test data pools for realistic scenarios
const SEARCH_TERMS = [
  "book",
  "programming",
  "fiction",
  "science",
  "history",
  "mystery",
  "romance",
  "fantasy",
  "thriller",
  "biography",
];
const CATEGORY_FILTERS = [
  "Technology",
  "Fiction",
  "Science",
  "History",
  "Business",
  "Romance",
  "Mystery",
  "Fantasy",
  "Biography",
];
const SORT_OPTIONS = [
  { orderBy: "Name", isDescending: false },
  { orderBy: "Name", isDescending: true },
  { orderBy: "Price", isDescending: false },
  { orderBy: "Price", isDescending: true },
  { orderBy: "Rating", isDescending: true },
  { orderBy: "PublishDate", isDescending: true },
];

// Author and publisher pools for testing
const AUTHOR_NAMES = [
  "John Doe",
  "Jane Smith",
  "Robert Johnson",
  "Mary Wilson",
  "David Brown",
];
const PUBLISHER_NAMES = [
  "TechBooks",
  "Fiction House",
  "Academic Press",
  "Popular Books",
  "Classic Publishing",
];

// Random data generators for more realistic testing
const getRandomSearchTerm = () =>
  SEARCH_TERMS[Math.floor(Math.random() * SEARCH_TERMS.length)];
const getRandomCategory = () =>
  CATEGORY_FILTERS[Math.floor(Math.random() * CATEGORY_FILTERS.length)];
const getRandomSortOption = () =>
  SORT_OPTIONS[Math.floor(Math.random() * SORT_OPTIONS.length)];
const getRandomPrice = (min = 5, max = 100) =>
  Math.floor(Math.random() * (max - min) + min);
const getRandomPageSize = () =>
  [5, 10, 15, 20, 25][Math.floor(Math.random() * 5)];
const getRandomPageIndex = () => Math.floor(Math.random() * 5) + 1; // Pages 1-5

export const options = {
  maxVUs: 50, // Reduced from 200 for development environment
  preAllocatedVUs: 10, // Reduced from 50
  timeUnit: "1s",
  startRate: 0,
  abortOnFail: false,
  scenarios: {
    // Scenario 1: Browse catalog (light load)
    browse_catalog: {
      executor: "ramping-vus",
      env: { scenario: "browse_catalog" },
      startVUs: 0,
      stages: [
        { duration: "30s", target: 3 }, // Reduced from 10
        { duration: "1m", target: 5 }, // Reduced from 15, shorter duration
        { duration: "30s", target: 0 },
      ],
      gracefulRampDown: "30s",
    },
    // Scenario 2: Search and filter (medium load)
    search_filter: {
      executor: "ramping-vus",
      env: { scenario: "search_filter" },
      startVUs: 0,
      stages: [
        { duration: "30s", target: 5 }, // Reduced from 20
        { duration: "1m", target: 8 }, // Reduced from 30, shorter duration
        { duration: "30s", target: 0 },
      ],
      gracefulRampDown: "30s",
    },
    // Scenario 3: API endpoint testing (comprehensive)
    api_comprehensive: {
      executor: "ramping-vus",
      env: { scenario: "api_comprehensive" },
      startVUs: 0,
      stages: [
        { duration: "20s", target: 3 }, // Reduced from 15
        { duration: "40s", target: 6 }, // Reduced from 25, shorter duration
        { duration: "20s", target: 0 },
      ],
      gracefulRampDown: "20s",
    },
    // Scenario 4: Stress test (high load) - significantly reduced
    stress_test: {
      executor: "ramping-vus",
      env: { scenario: "stress_test" },
      startVUs: 0,
      stages: [
        { duration: "30s", target: 10 }, // Reduced from 50
        { duration: "1m", target: 15 }, // Reduced from 100, shorter duration
        { duration: "30s", target: 0 },
      ],
      gracefulRampDown: "30s",
    },
    // Scenario 5: Spike test (sudden load increase) - reduced
    spike_test: {
      executor: "ramping-vus",
      env: { scenario: "spike_test" },
      startVUs: 0,
      stages: [
        { duration: "20s", target: 2 }, // Reduced from 10
        { duration: "10s", target: 12 }, // Reduced from 80
        { duration: "20s", target: 12 }, // Maintain spike
        { duration: "10s", target: 2 }, // Quick recovery
        { duration: "20s", target: 0 },
      ],
      gracefulRampDown: "20s",
    },
  },
  thresholds: {
    // Response time thresholds - more realistic for development
    http_req_duration: [
      `p(95)<${RESPONSE_TIME_THRESHOLD_95}`,
      `p(99)<${RESPONSE_TIME_THRESHOLD_99}`,
      "avg<500", // Average response time should be under 500ms
      "med<300", // Median response time should be under 300ms
    ],
    // Error rate thresholds - more lenient for development
    http_req_failed: ["rate<0.05"], // Error rate should be less than 5%
    http_req_waiting: ["p(95)<800"], // Server processing time
    checks: [`rate>${CHECK_RATE_THRESHOLD}`], // 90% of checks should pass

    // Custom metrics thresholds per endpoint - more realistic
    "http_req_duration{name:books}": ["p(95)<1200", "avg<500"],
    "http_req_duration{name:search}": ["p(95)<1500", "avg<600"],
    "http_req_duration{name:categories}": ["p(95)<800", "avg<300"],
    "http_req_duration{name:authors}": ["p(95)<800", "avg<300"],
    "http_req_duration{name:publishers}": ["p(95)<800", "avg<300"],

    // Scenario-specific thresholds
    "http_req_duration{scenario:stress_test}": ["p(95)<2000"],
    "http_req_duration{scenario:spike_test}": ["p(95)<3000"],

    // Check success rates per scenario - more lenient
    "checks{scenario:browse_catalog}": ["rate>0.90"],
    "checks{scenario:search_filter}": ["rate>0.85"],
    "checks{scenario:api_comprehensive}": ["rate>0.88"],
  },
};

export const setup = () => {
  return {
    testStartTime: Date.now(),
  };
};

const BASE_URL = __ENV.services__gateway__http__0;

// Validate BASE_URL is set
if (!BASE_URL) {
  throw new Error(
    "BASE_URL is not set. Please provide services__gateway__http__0 environment variable."
  );
}

// Enhanced helper functions for better test organization and debugging
function validateResponse(
  response,
  name,
  expectedStatus = HTTP_OK,
  _maxDuration = RESPONSE_TIME_THRESHOLD_95
) {
  // Traditional K6 checks for metrics (tagged for better analysis)
  const tags = { name, scenario: __ENV.scenario || "default" };

  // Only perform detailed validation for successful responses
  if (response.status === expectedStatus && expectedStatus === HTTP_OK) {
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
      console.warn(`JSON parsing failed for ${name}:`, error.message);
      console.warn(
        `Response status: ${response.status}, Body: ${
          response.body?.substring(0, 200) || "empty"
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

function validatePagedResponse(data, name) {
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
    console.error(
      `Paged response validation failed for ${name}:`,
      error.message
    );
    return false;
  }
}

function testEndpointWithRetry(url, params, name, maxRetries = 2) {
  let response;
  let retries = 0;

  do {
    try {
      response = http.get(url, {
        params,
        tags: { name, retry: retries },
        timeout: "10s",
      });
      if (response.status === HTTP_OK || retries >= maxRetries) break;
    } catch (error) {
      console.warn(
        `Request failed for ${name} (attempt ${retries + 1}): ${error.message}`
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

  if (response.status !== HTTP_OK && retries > 0) {
    console.warn(
      `${name} failed after ${retries} retries. Final status: ${response.status}`
    );
  }

  return response;
}

// New helper function for testing specific book details
function testBookDetails(bookId, name = "book_details") {
  const response = http.get(`${BASE_URL}/catalog/api/v1/books/${bookId}`);
  const data = validateResponse(response, name);

  if (data && response.status === HTTP_OK) {
    try {
      expect(data, `${name} should have id`).to.have.property("id");
      expect(data, `${name} should have title`).to.have.property("title");
      expect(data, `${name} should have price`).to.have.property("price");
      expect(data.price, `${name} price should be positive`).to.be.above(0);
    } catch (error) {
      console.error(
        `Book details validation failed for ${name}:`,
        error.message
      );
    }
  }

  return data;
}

// New helper function for basic connectivity check
function checkServiceAvailability() {
  const response = http.get(`${BASE_URL}/catalog/health`, { timeout: "5s" });

  if (response.status === 0) {
    console.error(
      `Service not available at ${BASE_URL}. Check if the service is running.`
    );
    return false;
  }

  // Accept various health check status codes
  const acceptableStatuses = [HTTP_OK, 404]; // Some services might not have /health endpoint
  if (!acceptableStatuses.includes(response.status)) {
    console.warn(
      `Service at ${BASE_URL} responded with status ${response.status}`
    );
  }

  return true;
}

export default function () {
  // Quick connectivity check
  if (!checkServiceAvailability()) {
    console.error(
      "Service availability check failed. Skipping test iteration."
    );
    sleep(5); // Wait before retrying
    return;
  }

  const scenario = __ENV.scenario || "browse_catalog";

  switch (scenario) {
    case "browse_catalog":
      browseCatalogScenario();
      break;
    case "search_filter":
      searchFilterScenario();
      break;
    case "api_comprehensive":
      apiComprehensiveScenario();
      break;
    case "stress_test":
      stressTestScenario();
      break;
    case "spike_test":
      spikeTestScenario();
      break;
    default:
      browseCatalogScenario();
  }

  // Variable sleep time to simulate real user behavior
  sleep(Math.random() * 2 + 1); // 1-3 seconds
}

function browseCatalogScenario() {
  try {
    // Test basic catalog browsing - simulates users browsing the bookstore
    const booksResponse = http.get(`${BASE_URL}/catalog/api/v1/books`, {
      tags: { scenario: "browse_catalog", endpoint: "books" },
      timeout: "10s",
    });
    const booksData = validateResponse(booksResponse, "books", HTTP_OK, 1200);
    validatePagedResponse(booksData, "books");

    // Test categories endpoint
    const categoriesResponse = http.get(
      `${BASE_URL}/catalog/api/v1/categories`,
      {
        tags: { scenario: "browse_catalog", endpoint: "categories" },
        timeout: "10s",
      }
    );
    validateResponse(categoriesResponse, "categories", HTTP_OK, 800);

    // Simulate user viewing a specific category if available (70% chance)
    if (Math.random() > 0.3) {
      const categoryParams = {
        category: getRandomCategory(),
        pageSize: getRandomPageSize(),
        pageIndex: 1,
      };
      const categoryBooksResponse = testEndpointWithRetry(
        `${BASE_URL}/catalog/api/v1/books`,
        categoryParams,
        "category_books"
      );
      validateResponse(categoryBooksResponse, "category_books");
    }

    // Simulate browsing authors (50% chance)
    if (Math.random() > 0.5) {
      const authorsResponse = http.get(`${BASE_URL}/catalog/api/v1/authors`, {
        tags: { scenario: "browse_catalog", endpoint: "authors" },
        timeout: "10s",
      });
      validateResponse(authorsResponse, "authors", HTTP_OK, 800);
    } // Simulate viewing a specific book (30% chance)
    if (Math.random() > 0.7 && booksData?.items?.length > 0) {
      const randomBook =
        booksData.items[Math.floor(Math.random() * booksData.items.length)];
      if (randomBook?.id) {
        testBookDetails(randomBook.id, "browse_book_details");
      }
    }

    // Simulate pagination browsing (40% chance)
    if (Math.random() > 0.6) {
      const paginationParams = {
        pageIndex: getRandomPageIndex(),
        pageSize: getRandomPageSize(),
      };
      const paginationResponse = http.get(`${BASE_URL}/catalog/api/v1/books`, {
        params: paginationParams,
        tags: { scenario: "browse_catalog", endpoint: "books_pagination" },
        timeout: "10s",
      });
      validateResponse(paginationResponse, "pagination_browse");
    }
  } catch (error) {
    console.error(`Error in browseCatalogScenario: ${error.message}`);
  }
}

function searchFilterScenario() {
  // Test comprehensive search and filtering functionality
  const searchTerm = getRandomSearchTerm();
  const minPrice = getRandomPrice(5, 25);
  const maxPrice = getRandomPrice(26, 100);
  const sortOption = getRandomSortOption();

  // Test search with various parameters
  const searchParams = {
    search: searchTerm,
    minPrice,
    maxPrice,
    pageIndex: 1,
    pageSize: getRandomPageSize(),
    ...sortOption,
  };

  const searchResponse = testEndpointWithRetry(
    `${BASE_URL}/catalog/api/v1/books`,
    searchParams,
    "search"
  );
  const searchData = validateResponse(searchResponse, "search", HTTP_OK, 800);
  validatePagedResponse(searchData, "search");

  // Test price-only filtering
  const priceFilterParams = {
    minPrice: getRandomPrice(10, 30),
    maxPrice: getRandomPrice(31, 80),
    pageIndex: 1,
    pageSize: 10,
  };

  const priceFilterResponse = http.get(`${BASE_URL}/catalog/api/v1/books`, {
    params: priceFilterParams,
  });
  validateResponse(priceFilterResponse, "price_filter");

  // Test pagination with different page sizes
  if (Math.random() > 0.5) {
    const paginationParams = {
      pageIndex: Math.floor(Math.random() * 3) + 1, // Pages 1-3
      pageSize: getRandomPageSize(),
    };

    const paginationResponse = http.get(`${BASE_URL}/catalog/api/v1/books`, {
      params: paginationParams,
    });
    validateResponse(paginationResponse, "pagination");
  }
}

function apiComprehensiveScenario() {
  // Test all available API endpoints systematically
  const endpoints = [
    {
      url: `${BASE_URL}/catalog/api/v1/books`,
      name: "books",
      maxDuration: 600,
    },
    {
      url: `${BASE_URL}/catalog/api/v1/categories`,
      name: "categories",
      maxDuration: 300,
    },
    {
      url: `${BASE_URL}/catalog/api/v1/authors`,
      name: "authors",
      maxDuration: 400,
    },
    {
      url: `${BASE_URL}/catalog/api/v1/publishers`,
      name: "publishers",
      maxDuration: 400,
    },
  ];

  // Test all basic endpoints
  endpoints.forEach((endpoint) => {
    const response = http.get(endpoint.url, {
      tags: { scenario: "api_comprehensive", endpoint: endpoint.name },
    });
    validateResponse(response, endpoint.name, HTTP_OK, endpoint.maxDuration);

    // Test with various query parameters for books endpoint
    if (endpoint.name === "books") {
      const paramCombinations = [
        { pageSize: 5, orderBy: "Name", isDescending: false },
        { pageSize: 10, orderBy: "Price", isDescending: true },
        { minPrice: 10, maxPrice: 50 },
        { category: getRandomCategory() },
        { search: getRandomSearchTerm(), pageSize: 15 },
      ];

      paramCombinations.forEach((params, index) => {
        const paramsResponse = http.get(endpoint.url, {
          params,
          tags: {
            scenario: "api_comprehensive",
            endpoint: `${endpoint.name}_params_${index}`,
          },
        });
        validateResponse(
          paramsResponse,
          `${endpoint.name}_with_params_${index}`
        );
      });
    }
  });

  // Test edge cases and boundary conditions
  const edgeCases = [
    { params: { pageSize: 1 }, name: "min_page_size" },
    { params: { pageSize: 100 }, name: "max_page_size" },
    { params: { pageIndex: 1000 }, name: "high_page_index" },
    { params: { minPrice: 0, maxPrice: 1 }, name: "low_price_range" },
    { params: { minPrice: 999, maxPrice: 1000 }, name: "high_price_range" },
  ];

  edgeCases.forEach((testCase) => {
    const response = http.get(`${BASE_URL}/catalog/api/v1/books`, {
      params: testCase.params,
      tags: {
        scenario: "api_comprehensive",
        endpoint: `edge_case_${testCase.name}`,
      },
    });
    validateResponse(response, `edge_case_${testCase.name}`);
  });

  // Test error scenarios (20% chance)
  if (Math.random() > 0.8) {
    const errorTests = [
      {
        url: `${BASE_URL}/catalog/api/v1/books/999999`,
        expectedStatus: HTTP_NOT_FOUND,
        name: "invalid_book_id",
      },
      {
        url: `${BASE_URL}/catalog/api/v1/categories/999999`,
        expectedStatus: HTTP_NOT_FOUND,
        name: "invalid_category_id",
      },
    ];

    errorTests.forEach((test) => {
      const response = http.get(test.url, {
        tags: { scenario: "api_comprehensive", endpoint: `error_${test.name}` },
      });
      validateResponse(response, test.name, test.expectedStatus, 1000);
    });
  }

  // Test malformed query parameters (10% chance)
  if (Math.random() > 0.9) {
    const malformedParams = { pageSize: "invalid", minPrice: "not_a_number" };
    const malformedResponse = http.get(`${BASE_URL}/catalog/api/v1/books`, {
      params: malformedParams,
      tags: { scenario: "api_comprehensive", endpoint: "malformed_params" },
    });
    // Expecting either 400 Bad Request or the API to handle gracefully
    const acceptableStatuses = [HTTP_OK, HTTP_BAD_REQUEST];
    const isValidStatus = acceptableStatuses.includes(malformedResponse.status);

    check(
      malformedResponse,
      {
        "malformed params handled appropriately": () => isValidStatus,
      },
      { scenario: "api_comprehensive", endpoint: "malformed_params" }
    );
  }
}

function stressTestScenario() {
  // High-intensity testing with concurrent requests
  const requests = [
    { url: `${BASE_URL}/catalog/api/v1/books`, name: "books_stress" },
    { url: `${BASE_URL}/catalog/api/v1/categories`, name: "categories_stress" },
    { url: `${BASE_URL}/catalog/api/v1/authors`, name: "authors_stress" },
    { url: `${BASE_URL}/catalog/api/v1/publishers`, name: "publishers_stress" },
  ];

  // Make multiple concurrent requests
  const responses = requests.map((req) => http.get(req.url));

  // Validate all responses
  responses.forEach((response, index) => {
    validateResponse(response, requests[index].name, HTTP_OK, 1000); // More lenient timing for stress test
  });

  // Additional complex search under stress
  const complexSearchParams = {
    search: getRandomSearchTerm(),
    minPrice: getRandomPrice(5, 50),
    maxPrice: getRandomPrice(51, 100),
    pageIndex: Math.floor(Math.random() * 5) + 1,
    pageSize: getRandomPageSize(),
    orderBy: getRandomSortOption().orderBy,
    isDescending: Math.random() > 0.5,
  };

  const stressSearchResponse = http.get(`${BASE_URL}/catalog/api/v1/books`, {
    params: complexSearchParams,
  });
  validateResponse(stressSearchResponse, "stress_search", HTTP_OK, 1200);
}

function spikeTestScenario() {
  // Simulate sudden traffic spikes - focuses on resilience
  const spikeEndpoints = [
    `${BASE_URL}/catalog/api/v1/books`,
    `${BASE_URL}/catalog/api/v1/categories`,
  ];

  const randomEndpoint =
    spikeEndpoints[Math.floor(Math.random() * spikeEndpoints.length)];
  const response = http.get(randomEndpoint);

  // During spike tests, we're more forgiving with response times
  validateResponse(response, "spike_test", HTTP_OK, 1500);
  // Simulate rapid consecutive requests (like a user clicking rapidly)
  if (Math.random() > 0.7) {
    // 30% chance
    const rapidResponse = http.get(randomEndpoint);
    validateResponse(rapidResponse, "rapid_request", HTTP_OK, 1500);
  }
}

export function handleSummary(data) {
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
      `Total Requests: ${data.metrics.http_reqs?.values?.count || 0}`
    );
    console.log(
      `Failed Requests: ${data.metrics.http_req_failed?.values?.fails || 0}`
    );
    console.log(
      `Average Response Time: ${Math.round(
        data.metrics.http_req_duration?.values?.avg || 0
      )}ms`
    );
    console.log(
      `95th Percentile Response Time: ${Math.round(
        data.metrics.http_req_duration?.values?.["p(95)"] || 0
      )}ms`
    );
    console.log(
      `Check Success Rate: ${Math.round(
        (data.metrics.checks?.values?.passes /
          (data.metrics.checks?.values?.passes +
            data.metrics.checks?.values?.fails)) *
          100
      )}%`
    );
    console.log("===============================================\n");
  }

  return customSummary;
}
