import react from "@vitejs/plugin-react";
import crypto from "node:crypto";
import os from "node:os";
import path from "node:path";
import process from "node:process";
import { defineConfig } from "vitest/config";

// Set unique MSW cookie store path BEFORE any test file imports MSW.
// This must live here (not in setup.ts) because JS import hoisting causes
// MSW's CookieStore to initialise before module-level statements in setup
// files run, leading to a shared SQLite file and "database is locked" errors
// when Turbo executes both app test suites in parallel.
process.env.MSW_COOKIE_STORE_PATH ??= path.resolve(
  os.tmpdir(),
  `msw-cookies-backoffice-${process.pid}-${crypto.randomUUID()}.db`,
);

export default defineConfig({
  plugins: [react()],
  test: {
    globals: true,
    environment: "happy-dom",
    execArgv: [
      "--localstorage-file",
      path.resolve(os.tmpdir(), `vitest-${process.pid}.localstorage`),
    ],
    setupFiles: ["./__tests__/setup.ts"],
    include: ["**/__tests__/**/*.test.{ts,tsx}", "**/*.test.{ts,tsx}"],
    exclude: ["node_modules", ".next", "e2e"],
    coverage: {
      provider: "v8",
      reporter: ["text", "json", "json-summary", "html", "lcov"],
      reportsDirectory: "./coverage",
      exclude: [
        "node_modules/",
        ".next/",
        "__tests__/",
        "e2e/",
        "**/*.config.*",
        "**/*.d.ts",
        "**/types.ts",
        // Pages & App Directory (integration tested separately)
        "app/**/*.tsx",
        "app/**/*.ts",
        "**/layout.tsx",
        "**/not-found.tsx",
        "**/page.tsx",
        "**/loading.tsx",
        "**/error.tsx",
        "**/template.tsx",
        // Infrastructure & Setup
        "**/*.mjs",
        "**/env.mjs",
        "**/instrumentation.ts",
        "**/instrumentation.node.ts",
        "**/middleware.ts",
        // Query Client (config wrapper)
        "lib/query-client.ts",
        "lib/constants.ts",
        // Auth & Providers (integration concerns)
        "lib/auth.ts",
        "lib/auth-client.ts",
        "app/providers.tsx",
        // MSW & Mocking
        "lib/msw.ts",
        "public/mockServiceWorker.js",
        // Complex UI Components (should be e2e tested)
        "components/dashboard-header.tsx",
        "components/dashboard-nav.tsx",
        "components/auth-guard.tsx",
        "components/mobile-blocker.tsx",
        // Loading Skeletons (visual only)
        "components/loading-skeleton.tsx",
        "**/loading-skeleton.tsx",
      ],
    },
    pool: "forks",
    maxWorkers: 1,
    isolate: false,
  },
  resolve: {
    alias: {
      "@": path.resolve(__dirname, "./"),
      "@workspace/ui": path.resolve(__dirname, "../../packages/ui/src"),
      "@workspace/api-hooks": path.resolve(
        __dirname,
        "../../packages/api-hooks/src",
      ),
      "@workspace/types": path.resolve(__dirname, "../../packages/types/src"),
      "@workspace/api-client": path.resolve(
        __dirname,
        "../../packages/api-client/src",
      ),
      "@workspace/validations": path.resolve(
        __dirname,
        "../../packages/validations/src",
      ),
      "@workspace/mocks": path.resolve(__dirname, "../../packages/mocks/src"),
      "@workspace/utils": path.resolve(__dirname, "../../packages/utils/src"),
    },
  },
});
