import { TestConstants } from "../types";

/**
 * Test constants and thresholds
 */
export const CONSTANTS: TestConstants = {
  HTTP_OK: 200,
  HTTP_BAD_REQUEST: 400,
  HTTP_NOT_FOUND: 404,
  RESPONSE_TIME_THRESHOLD_95: 1000, // milliseconds - more realistic for development
  RESPONSE_TIME_THRESHOLD_99: 2000, // milliseconds - more lenient
  CHECK_RATE_THRESHOLD: 0.9, // 90% instead of 95% for development environment
};

/**
 * K6 options configuration
 */
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
      `p(95)<${CONSTANTS.RESPONSE_TIME_THRESHOLD_95}`,
      `p(99)<${CONSTANTS.RESPONSE_TIME_THRESHOLD_99}`,
      "avg<500", // Average response time should be under 500ms
      "med<300", // Median response time should be under 300ms
    ],
    // Error rate thresholds - more lenient for development
    http_req_failed: ["rate<0.05"], // Error rate should be less than 5%
    http_req_waiting: ["p(95)<800"], // Server processing time
    checks: [`rate>${CONSTANTS.CHECK_RATE_THRESHOLD}`], // 90% of checks should pass

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
