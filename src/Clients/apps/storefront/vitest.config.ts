import react from "@vitejs/plugin-react";
import { defineConfig } from "vitest/config";

import { createVitestConfig } from "@workspace/app-config/vitest";

export default defineConfig(
  createVitestConfig({
    appName: "storefront",
    epic: "Storefront",
    application: "@bookworm/storefront",
    appDir: import.meta.dirname,
    plugins: [react()],
    extraCoverageExclude: [
      "**/proxy.ts",
      "lib/policy-content.ts",
      "**/robots.ts",
      "**/sitemap.ts",
      "**/manifest.ts",
      "**/opengraph-image.tsx",
      "lib/seo.ts",
      "components/providers.tsx",
      "hooks/useUserContext.ts",
      "components/back-to-top.tsx",
      "components/footer-policies.tsx",
      "components/header.tsx",
      "components/footer.tsx",
      "components/mobile-bottom-nav.tsx",
      "components/chat-bot.tsx",
      "components/policy-dialog.tsx",
      "features/content/**",
      "features/home/**",
    ],
  }),
);
