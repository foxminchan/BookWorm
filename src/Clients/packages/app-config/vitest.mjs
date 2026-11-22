import crypto from "node:crypto";
import os from "node:os";
import path from "node:path";

const SHARED_COVERAGE_EXCLUDE = [
  "node_modules/",
  ".next/",
  "__tests__/",
  "e2e/",
  "**/*.config.*",
  "**/*.d.ts",
  "**/types.ts",
  "app/**/*.tsx",
  "app/**/*.ts",
  "**/layout.tsx",
  "**/not-found.tsx",
  "**/page.tsx",
  "**/loading.tsx",
  "**/error.tsx",
  "**/template.tsx",
  "**/*.mjs",
  "**/env.mjs",
  "**/instrumentation.ts",
  "**/instrumentation.node.ts",
  "**/middleware.ts",
  "lib/constants.ts",
  "lib/query-client.ts",
  "lib/auth.ts",
  "lib/auth-client.ts",
  "lib/msw.ts",
  "public/mockServiceWorker.js",
  "components/loading-skeleton.tsx",
  "**/loading-skeleton.tsx",
];

const WORKSPACE_PACKAGES = [
  "ui",
  "api-hooks",
  "types",
  "api-client",
  "validations",
  "mocks",
  "utils",
  "auth",
  "seo",
  "env",
  "otel",
  "storefront-shell",
];

/**
 * Creates a shared Vitest configuration for BookWorm frontend applications.
 * @param {{
 *   appName: string;
 *   epic: string;
 *   application: string;
 *   appDir: string;
 *   plugins: unknown[];
 *   extraCoverageExclude?: string[];
 *   maxWorkers?: number;
 * }} options
 * @returns {any}
 */
export function createVitestConfig({
  appName,
  epic,
  application,
  appDir,
  plugins,
  extraCoverageExclude = [],
  maxWorkers,
}) {
  // Set unique MSW cookie store path BEFORE any test file imports MSW.
  // JS import hoisting causes MSW's CookieStore to initialise before setup
  // files run, leading to "database is locked" errors in parallel Turbo runs.
  process.env.MSW_COOKIE_STORE_PATH ??= path.resolve(
    os.tmpdir(),
    `msw-cookies-${appName}-${process.pid}-${crypto.randomUUID()}.db`,
  );

  const alias = {
    "@": path.resolve(appDir, "./"),
  };
  for (const pkg of WORKSPACE_PACKAGES) {
    alias[`@workspace/${pkg}`] = path.resolve(
      appDir,
      `../../packages/${pkg}/src`,
    );
  }

  return {
    plugins,
    test: {
      globals: true,
      environment: "happy-dom",
      reporters: [
        "default",
        [
          "allure-vitest/reporter",
          {
            resultsDir: "./allure-results",
            globalLabels: {
              layer: "unit",
              epic,
              severity: "normal",
              owner: "BookWorm Engineering",
            },
            environmentInfo: {
              Application: application,
              Framework: "Next.js 16 / React 19",
              Runner: "Vitest",
              Environment: "Happy DOM",
            },
            links: {
              issue: {
                nameTemplate: "Issue #%s",
                urlTemplate:
                  "https://github.com/foxminchan/BookWorm/issues/%s",
              },
              tms: {
                nameTemplate: "TMS #%s",
                urlTemplate:
                  "https://github.com/foxminchan/BookWorm/issues/%s",
              },
            },
          },
        ],
      ],
      setupFiles: ["allure-vitest/setup", "./__tests__/setup.ts"],
      include: ["**/__tests__/**/*.test.{ts,tsx}", "**/*.test.{ts,tsx}"],
      exclude: ["node_modules", ".next", "e2e"],
      coverage: {
        provider: "v8",
        reporter: ["text", "json", "json-summary", "html", "lcov"],
        reportsDirectory: "./coverage",
        exclude: [...SHARED_COVERAGE_EXCLUDE, ...extraCoverageExclude],
      },
      pool: "forks",
      ...(maxWorkers !== undefined ? { maxWorkers } : {}),
      execArgv: [
        "--localstorage-file",
        path.resolve(os.tmpdir(), `vitest-${crypto.randomUUID()}.localstorage`),
      ],
      isolate: false,
    },
    resolve: { alias },
  };
}
