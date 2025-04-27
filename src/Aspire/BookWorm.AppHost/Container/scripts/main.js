import http from "k6/http";
import { check, sleep } from "k6";
import { textSummary } from "https://jslib.k6.io/k6-summary/0.0.1/index.js";
import { htmlReport } from "https://raw.githubusercontent.com/benc-uk/k6-reporter/main/dist/bundle.js";

// Constants
const HTTP_OK = 200;
const RESPONSE_TIME_THRESHOLD_95 = 500; // milliseconds
const RESPONSE_TIME_THRESHOLD_99 = 1000; // milliseconds
const CHECK_RATE_THRESHOLD = 0.95;

export const options = {
  maxVUs: 100,
  preAllocatedVUs: 20,
  timeUnit: "1s",
  startRate: 0,
  abortOnFail: false,
  scenarios: {
    // Scenario 1: Browse catalog (light load)
    browse_catalog: {
      executor: "ramping-vus",
      startVUs: 0,
      stages: [
        {
          duration: "30s",
          target: 10,
        },
        {
          duration: "1m",
          target: 10,
        },
        {
          duration: "30s",
          target: 0,
        },
      ],
      gracefulRampDown: "30s",
    },
    // Scenario 2: Search and filter (medium load)
    search_filter: {
      executor: "ramping-vus",
      startVUs: 0,
      stages: [
        {
          duration: "30s",
          target: 20,
        },
        {
          duration: "1m",
          target: 20,
        },
        {
          duration: "30s",
          target: 0,
        },
      ],
      gracefulRampDown: "30s",
    },
    // Scenario 3: Stress test (high load)
    stress_test: {
      executor: "ramping-vus",
      startVUs: 0,
      stages: [
        {
          duration: "30s",
          target: 50,
        },
        {
          duration: "1m",
          target: 50,
        },
        {
          duration: "30s",
          target: 0,
        },
      ],
      gracefulRampDown: "30s",
    },
  },
  thresholds: {
    http_req_duration: [
      `p(95)<${RESPONSE_TIME_THRESHOLD_95}`,
      `p(99)<${RESPONSE_TIME_THRESHOLD_99}`,
      "avg<8",
    ],
    http_req_failed: ["rate<0.01"],
    checks: [`rate>${CHECK_RATE_THRESHOLD}`],
  },
};

export const setup = () => {
  return {
    testStartTime: Date.now(),
  };
};

const BASE_URL = __ENV.services__gateway__http__0;

export default function () {
  // Scenario 1: Browse catalog
  if (__ENV.scenario === "browse_catalog") {
    const booksResponse = http.get(`${BASE_URL}/catalog/books`);
    check(booksResponse, {
      "books status is 200": (r) => r.status === HTTP_OK,
      "books response time < 500ms": (r) =>
        r.timings.duration < RESPONSE_TIME_THRESHOLD_95,
    });

    const categoriesResponse = http.get(`${BASE_URL}/catalog/categories`);
    check(categoriesResponse, {
      "categories status is 200": (r) => r.status === HTTP_OK,
      "categories response time < 500ms": (r) =>
        r.timings.duration < RESPONSE_TIME_THRESHOLD_95,
    });
  }

  // Scenario 2: Search and filter
  if (__ENV.scenario === "search_filter") {
    // Test search with different parameters
    const searchParams = {
      search: "book",
      minPrice: 10,
      maxPrice: 50,
      pageIndex: 1,
      pageSize: 10,
      orderBy: "Name",
      isDescending: false,
    };

    const searchResponse = http.get(`${BASE_URL}/catalog/books`, {
      params: searchParams,
    });
    check(searchResponse, {
      "search status is 200": (r) => r.status === HTTP_OK,
      "search response time < 500ms": (r) =>
        r.timings.duration < RESPONSE_TIME_THRESHOLD_95,
    });

    // Test filtering by price range
    const priceFilterParams = {
      minPrice: 20,
      maxPrice: 30,
      pageIndex: 1,
      pageSize: 10,
    };

    const priceFilterResponse = http.get(`${BASE_URL}/catalog/books`, {
      params: priceFilterParams,
    });
    check(priceFilterResponse, {
      "price filter status is 200": (r) => r.status === HTTP_OK,
      "price filter response time < 500ms": (r) =>
        r.timings.duration < RESPONSE_TIME_THRESHOLD_95,
    });

    // Test sorting
    const sortParams = {
      orderBy: "Name",
      isDescending: true,
      pageIndex: 1,
      pageSize: 10,
    };

    const sortResponse = http.get(`${BASE_URL}/catalog/books`, {
      params: sortParams,
    });
    check(sortResponse, {
      "sort status is 200": (r) => r.status === HTTP_OK,
      "sort response time < 500ms": (r) =>
        r.timings.duration < RESPONSE_TIME_THRESHOLD_95,
    });

    // Test pagination
    const paginationParams = {
      pageIndex: 2,
      pageSize: 5,
    };

    const paginationResponse = http.get(`${BASE_URL}/catalog/books`, {
      params: paginationParams,
    });
    check(paginationResponse, {
      "pagination status is 200": (r) => r.status === HTTP_OK,
      "pagination response time < 500ms": (r) =>
        r.timings.duration < RESPONSE_TIME_THRESHOLD_95,
    });
  }

  // Scenario 3: Stress test
  if (__ENV.scenario === "stress_test") {
    // Make multiple concurrent requests
    const booksResponse = http.get(`${BASE_URL}/catalog/books`);
    const categoriesResponse = http.get(`${BASE_URL}/catalog/categories`);
    const authorsResponse = http.get(`${BASE_URL}/catalog/authors`);
    const publishersResponse = http.get(`${BASE_URL}/catalog/publishers`);

    check(booksResponse, {
      "books status is 200": (r) => r.status === HTTP_OK,
      "books response time < 500ms": (r) =>
        r.timings.duration < RESPONSE_TIME_THRESHOLD_95,
    });

    check(categoriesResponse, {
      "categories status is 200": (r) => r.status === HTTP_OK,
      "categories response time < 500ms": (r) =>
        r.timings.duration < RESPONSE_TIME_THRESHOLD_95,
    });

    check(authorsResponse, {
      "authors status is 200": (r) => r.status === HTTP_OK,
      "authors response time < 500ms": (r) =>
        r.timings.duration < RESPONSE_TIME_THRESHOLD_95,
    });

    check(publishersResponse, {
      "publishers status is 200": (r) => r.status === HTTP_OK,
      "publishers response time < 500ms": (r) =>
        r.timings.duration < RESPONSE_TIME_THRESHOLD_95,
    });
  }

  sleep(1);
}

export function handleSummary(data) {
  return {
    "summary.html": htmlReport(data),
    stdout: textSummary(data, { indent: " ", enableColors: true }),
  };
}
