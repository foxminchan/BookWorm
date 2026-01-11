import react from "@vitejs/plugin-react";
import os from "node:os";
import path from "node:path";
import process from "node:process";
import { defineConfig } from "vitest/config";

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
    // Set unique MSW cookie database per worker to avoid lock contention
    env: {
      MSW_COOKIE_STORE_PATH: path.resolve(
        os.tmpdir(),
        `msw-cookies-${process.pid}-${Date.now()}.db`,
      ),
    },
    coverage: {
      provider: "v8",
      reporter: ["text", "json", "json-summary", "html", "lcov"],
      reportsDirectory: "./coverage",
      exclude: [
        "node_modules/",
        ".next/",
        "__tests__/",
        "**/*.config.*",
        "**/*.d.ts",
        "**/types.ts",
        "e2e/",
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
        "**/proxy.ts",
        "**/middleware.ts",
        // SEO & Metadata
        "**/robots.ts",
        "**/sitemap.ts",
        "**/manifest.ts",
        "**/opengraph-image.tsx",
        "lib/seo.ts",
        // Auth & Providers (integration concerns)
        "lib/auth.ts",
        "lib/auth-client.ts",
        "components/providers.tsx",
        // MSW & Mocking
        "lib/msw.ts",
        "public/mockServiceWorker.js",
        // Complex UI Components (e2e tested)
        "components/header.tsx",
        "components/footer.tsx",
        "components/mobile-bottom-nav.tsx",
        "components/chat-bot.tsx",
        "components/policy-dialog.tsx",
        // Loading Skeletons (visual only)
        "components/loading-skeleton.tsx",
        "**/loading-skeleton.tsx",
        // Content Pages (static content)
        "features/content/**",
        "features/home/**",
      ],
    },
    pool: "threads",
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
