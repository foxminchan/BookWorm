import react from "@vitejs/plugin-react";
import { defineConfig } from "vitest/config";

import { createVitestConfig } from "@workspace/app-config/vitest";

export default defineConfig(
  createVitestConfig({
    appName: "commerce",
    epic: "Commerce",
    application: "@bookworm/commerce",
    appDir: import.meta.dirname,
    plugins: [react()],
    extraCoverageExclude: [
      "**/proxy.ts",
      "features/account/account-page-client.tsx",
      "features/basket/basket-page-client.tsx",
      "features/ordering/checkout/confirmation-page-client.tsx",
      "features/ordering/order-detail/order-detail-page-client.tsx",
      "features/ordering/orders/orders-page-client.tsx",
    ],
  }),
);
