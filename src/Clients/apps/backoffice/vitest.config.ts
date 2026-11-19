import react from "@vitejs/plugin-react";
import { defineConfig } from "vitest/config";

import { createVitestConfig } from "@workspace/app-config/vitest";

export default defineConfig(
  createVitestConfig({
    appName: "backoffice",
    epic: "Backoffice",
    application: "@bookworm/backoffice",
    appDir: import.meta.dirname,
    plugins: [react()],
    maxWorkers: 1,
    extraCoverageExclude: [
      "app/providers.tsx",
      "components/dashboard-header.tsx",
      "components/dashboard-nav.tsx",
      "components/auth-guard.tsx",
      "components/mobile-blocker.tsx",
    ],
  }),
);
