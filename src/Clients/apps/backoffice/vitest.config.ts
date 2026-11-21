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
      "config/**",
      "components/dashboard-header.tsx",
      "components/dashboard-nav.tsx",
      "components/auth-guard.tsx",
      "components/mobile-blocker.tsx",
      "components/page-container.tsx",
      "features/overview/overview-dashboard.tsx",
      "features/books/books-listing.tsx",
      "features/catalog/**",
      "features/orders/orders-listing.tsx",
      "features/orders/order-detail-view.tsx",
      "features/reviews/reviews-listing.tsx",
    ],
  }),
);
